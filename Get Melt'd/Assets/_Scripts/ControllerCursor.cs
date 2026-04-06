using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ControllerCursor : MonoBehaviour
{
    [Header("Cursor Settings")]
    public float cursorSpeed = 800f;
    public RectTransform cursorTransform;

    [Header("Move Animation")]
    public float tiltAmount = 25f;
    public float tiltSpeed = 10f;
    public float stretchAmount = 0.2f;
    public float stretchSpeed = 8f;

    [Header("Idle Animation")]
    public float idleBobbingSpeed = 2f;
    public float idleBobbingAmount = 5f;
    public float idleTimeThreshold = 2f;
    public float idlePulseSpeed = 1.5f;
    public float idlePulseAmount = 0.08f;

    [Header("Click Animation")]
    public float clickScaleAmount = 0.6f;
    public float clickRecoverSpeed = 10f;

    [Header("Stick Settings")]
    public bool useRightStick = false;

    [Header("Slider Settings")]
    public float sliderAdjustSpeed = 1.5f;

    private Vector2 _cursorPosition;
    private Vector2 _moveInput;

    private float _idleTimer;
    private bool _isIdle;
    private float _bobOffset;

    private Vector3 _baseScale;
    private Quaternion _baseRotation;
    private float _currentTilt;
    private Vector3 _currentScale;
    private bool _isClickAnimating;
    private float _clickScaleTarget;

    private bool _controllerConnected = false;
    private bool _cursorAllowed = false;

    private Slider _heldSlider = null;
    private bool _isDraggingSlider = false;

    void Awake()
    {
        _cursorPosition = new Vector2(Screen.width / 2f, Screen.height / 2f);

        if (cursorTransform != null)
        {
            _baseScale = cursorTransform.localScale;
            _currentScale = _baseScale;
            _baseRotation = cursorTransform.rotation;

            Image cursorImage = cursorTransform.GetComponent<Image>();
            if (cursorImage != null)
                cursorImage.raycastTarget = false;

            cursorTransform.gameObject.SetActive(false);
        }

        _clickScaleTarget = 1f;
        _cursorAllowed = false;

        InputSystem.onDeviceChange += OnDeviceChange;
        _controllerConnected = Gamepad.all.Count > 0;
    }

    void Start()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "MainMenu")
        {
            _cursorAllowed = true;
            RefreshCursorVisibility();
        }
        else if (currentScene == "Cutscene")
        {
            _cursorAllowed = false;
            RefreshCursorVisibility();
        }
        // ✅ Game scenes: GameManager.Resume() owns visibility
    }

    void OnDestroy()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    public void SetCursorVisible(bool visible)
    {
        _cursorAllowed = visible;

        if (!_controllerConnected)
            _controllerConnected = Gamepad.all.Count > 0;

        RefreshCursorVisibility();
    }

    void RefreshCursorVisibility()
    {
        if (!_controllerConnected)
            _controllerConnected = Gamepad.all.Count > 0;

        bool shouldShow = _controllerConnected && _cursorAllowed;

        if (cursorTransform != null)
        {
            Canvas cursorCanvas = cursorTransform.GetComponentInParent<Canvas>();
            if (cursorCanvas != null)
            {
                cursorCanvas.overrideSorting = true;
                cursorCanvas.sortingOrder = 9999;
            }

            cursorTransform.gameObject.SetActive(shouldShow);

            if (shouldShow)
                _cursorPosition = new Vector2(Screen.width / 2f, Screen.height / 2f);
        }

        Cursor.visible = !_controllerConnected;
    }

    void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is Gamepad)
        {
            switch (change)
            {
                case InputDeviceChange.Added:
                case InputDeviceChange.Reconnected:
                    _controllerConnected = true;
                    break;

                case InputDeviceChange.Removed:
                case InputDeviceChange.Disconnected:
                    _controllerConnected = false;
                    break;
            }
            RefreshCursorVisibility();
        }
    }

    void Update()
    {
        if (!_controllerConnected || !_cursorAllowed) return;

        if (Gamepad.current != null)
        {
            _moveInput = useRightStick
                ? Gamepad.current.rightStick.ReadValue()
                : Gamepad.current.leftStick.ReadValue();

            if (_moveInput.magnitude < 0.15f)
                _moveInput = Vector2.zero;

            if (_moveInput.magnitude > 0.1f)
            {
                _idleTimer = 0f;
                _isIdle = false;
                _bobOffset = 0f;
            }

            // ✅ Holding A + moving stick adjusts the slider
            if (_isDraggingSlider && _heldSlider != null)
            {
                _heldSlider.value += _moveInput.x * sliderAdjustSpeed * Time.unscaledDeltaTime;

                // ✅ Release slider when A is released
                if (Gamepad.current.buttonSouth.wasReleasedThisFrame)
                    ReleaseDragOnSlider();
            }
            else
            {
                // ✅ Press A to click or grab a slider
                if (Gamepad.current.buttonSouth.wasPressedThisFrame)
                {
                    TryClickOrGrabSlider();
                    TriggerClickAnimation();
                }
            }
        }

        MoveCursor();
        AnimateMoving();
        HandleIdleAnimation();
        HandleClickAnimation();
    }

    void TryClickOrGrabSlider()
    {
        if (EventSystem.current == null)
        {
            Debug.LogError("❌ No EventSystem found!");
            return;
        }

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = _cursorPosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        if (results.Count == 0)
        {
            Debug.LogWarning("⚠️ Raycast hit nothing!");
            return;
        }

        foreach (var result in results)
        {
            // ✅ Check if we hit a slider or any of its child objects
            Slider slider = result.gameObject.GetComponentInParent<Slider>();
            if (slider != null)
            {
                Debug.Log("🎚️ Grabbed slider: " + slider.name);
                _heldSlider = slider;
                _isDraggingSlider = true;

                ExecuteEvents.Execute(slider.gameObject, pointerData, ExecuteEvents.pointerDownHandler);
   
                ExecuteEvents.Execute(slider.gameObject, pointerData, ExecuteEvents.beginDragHandler);
                return;
            }

            // ✅ Normal button click
            Debug.Log("🖱️ Clicking: " + result.gameObject.name);
            ExecuteEvents.Execute(result.gameObject, pointerData, ExecuteEvents.pointerEnterHandler);
            ExecuteEvents.Execute(result.gameObject, pointerData, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(result.gameObject, pointerData, ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(result.gameObject, pointerData, ExecuteEvents.pointerClickHandler);
            break;
        }
    }

    void ReleaseDragOnSlider()
    {
        if (_heldSlider == null) return;

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = _cursorPosition
        };

        ExecuteEvents.Execute(_heldSlider.gameObject, pointerData, ExecuteEvents.endDragHandler);
        ExecuteEvents.Execute(_heldSlider.gameObject, pointerData, ExecuteEvents.pointerUpHandler);

        Debug.Log("🎚️ Released slider: " + _heldSlider.name);
        _heldSlider = null;
        _isDraggingSlider = false;
    }

    void TriggerClickAnimation()
    {
        _isClickAnimating = true;
        _clickScaleTarget = clickScaleAmount;
    }

    void MoveCursor()
    {
        _cursorPosition += _moveInput * cursorSpeed * Time.unscaledDeltaTime;
        _cursorPosition.x = Mathf.Clamp(_cursorPosition.x, 0, Screen.width);
        _cursorPosition.y = Mathf.Clamp(_cursorPosition.y, 0, Screen.height);

        if (!_isIdle && cursorTransform != null)
            cursorTransform.position = _cursorPosition;
    }

    void AnimateMoving()
    {
        if (cursorTransform == null) return;

        float speed = _moveInput.magnitude;
        bool isMoving = speed > 0.1f;

        float targetTilt = -_moveInput.x * tiltAmount;
        _currentTilt = Mathf.Lerp(_currentTilt, isMoving ? targetTilt : 0f, Time.unscaledDeltaTime * tiltSpeed);

        float targetStretchX = isMoving ? 1f - (stretchAmount * speed * Mathf.Abs(_moveInput.x)) : 1f;
        float targetStretchY = isMoving ? 1f + (stretchAmount * speed * Mathf.Abs(_moveInput.y)) : 1f;

        _currentScale.x = Mathf.Lerp(_currentScale.x, _baseScale.x * targetStretchX, Time.unscaledDeltaTime * stretchSpeed);
        _currentScale.y = Mathf.Lerp(_currentScale.y, _baseScale.y * targetStretchY, Time.unscaledDeltaTime * stretchSpeed);
        _currentScale.z = _baseScale.z;

        cursorTransform.rotation = _baseRotation * Quaternion.Euler(0f, 0f, _currentTilt);

        if (!_isClickAnimating)
            cursorTransform.localScale = _currentScale;
    }

    void HandleIdleAnimation()
    {
        if (cursorTransform == null) return;

        if (_moveInput.magnitude < 0.1f)
            _idleTimer += Time.unscaledDeltaTime;

        if (_idleTimer >= idleTimeThreshold)
            _isIdle = true;

        if (_isIdle)
        {
            _bobOffset += Time.unscaledDeltaTime * idleBobbingSpeed;

            float bob = Mathf.Sin(_bobOffset) * idleBobbingAmount;
            cursorTransform.position = new Vector3(_cursorPosition.x, _cursorPosition.y + bob, 0f);

            float pulse = 1f + Mathf.Sin(_bobOffset * idlePulseSpeed) * idlePulseAmount;
            if (!_isClickAnimating)
                cursorTransform.localScale = _baseScale * pulse;

            cursorTransform.rotation = _baseRotation * Quaternion.Euler(0f, 0f, Mathf.Lerp(_currentTilt, 0f, Time.unscaledDeltaTime * tiltSpeed));
        }
    }

    void HandleClickAnimation()
    {
        if (cursorTransform == null || !_isClickAnimating) return;

        float current = cursorTransform.localScale.x / _baseScale.x;
        float next = Mathf.Lerp(current, _clickScaleTarget, Time.unscaledDeltaTime * clickRecoverSpeed);
        cursorTransform.localScale = _baseScale * next;

        if (_clickScaleTarget < 1f && next <= clickScaleAmount + 0.05f)
            _clickScaleTarget = 1f;

        if (_clickScaleTarget >= 1f && next >= 0.98f)
        {
            cursorTransform.localScale = _baseScale;
            _isClickAnimating = false;
        }
    }
}