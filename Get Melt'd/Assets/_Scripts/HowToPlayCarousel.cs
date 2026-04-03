using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.InputSystem;
using TMPro;

public class HowToPlayCarousel : MonoBehaviour
{
    [System.Serializable]
    public class CarouselSlide
    {
        public VideoClip videoClip;

        [Header("Movement Caption")]
        [TextArea] public string keyboardCaption;
        [TextArea] public string controllerCaption;

        [Header("Jump Caption")]
        [TextArea] public string keyboardJumpCaption;
        [TextArea] public string controllerJumpCaption;
    }

    [Header("Slides")]
    public List<CarouselSlide> slides = new List<CarouselSlide>();

    [Header("UI References")]
    public VideoPlayer videoPlayer;
    public RawImage videoDisplay;
    public TMP_Text captionText;        // ✅ One shared movement caption TMP object
    public TMP_Text jumpCaptionText;    // ✅ One shared jump caption TMP object
    public Button prevButton;
    public Button nextButton;

    private int _currentIndex = 0;
    private RenderTexture _renderTexture;
    private bool _isControllerActive = false;

    void Awake()
    {
        _isControllerActive = Gamepad.all.Count > 0;
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    void Start()
    {
        _renderTexture = new RenderTexture(1920, 1080, 0);
        videoPlayer.targetTexture = _renderTexture;
        videoDisplay.texture = _renderTexture;

        prevButton.onClick.AddListener(PreviousSlide);
        nextButton.onClick.AddListener(NextSlide);

        if (slides.Count > 0)
            LoadSlide(_currentIndex);
    }

    void OnDestroy()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;

        if (_renderTexture != null)
        {
            _renderTexture.Release();
            Destroy(_renderTexture);
        }
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            SetInputMode(false);

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            SetInputMode(false);

        if (Gamepad.current != null)
        {
            Gamepad gp = Gamepad.current;
            if (gp.buttonSouth.wasPressedThisFrame ||
                gp.buttonNorth.wasPressedThisFrame ||
                gp.buttonEast.wasPressedThisFrame ||
                gp.buttonWest.wasPressedThisFrame ||
                gp.startButton.wasPressedThisFrame ||
                gp.selectButton.wasPressedThisFrame ||
                gp.leftShoulder.wasPressedThisFrame ||
                gp.rightShoulder.wasPressedThisFrame ||
                gp.leftStickButton.wasPressedThisFrame ||
                gp.rightStickButton.wasPressedThisFrame ||
                gp.leftTrigger.wasPressedThisFrame ||
                gp.rightTrigger.wasPressedThisFrame ||
                gp.dpad.up.wasPressedThisFrame ||
                gp.dpad.down.wasPressedThisFrame ||
                gp.dpad.left.wasPressedThisFrame ||
                gp.dpad.right.wasPressedThisFrame)
            {
                SetInputMode(true);
            }
        }
    }

    void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is Gamepad)
        {
            switch (change)
            {
                case InputDeviceChange.Added:
                case InputDeviceChange.Reconnected:
                    SetInputMode(true);
                    break;
                case InputDeviceChange.Removed:
                case InputDeviceChange.Disconnected:
                    SetInputMode(Gamepad.all.Count > 0);
                    break;
            }
        }
    }

    void SetInputMode(bool isController)
    {
        if (_isControllerActive == isController) return;
        _isControllerActive = isController;
        Debug.Log("🎮 Input mode: " + (_isControllerActive ? "Controller" : "Keyboard"));
        RefreshCaptions();
    }

    void RefreshCaptions()
    {
        if (_currentIndex < 0 || _currentIndex >= slides.Count) return;

        CarouselSlide slide = slides[_currentIndex];

        // ✅ Update shared movement caption
        if (captionText != null)
            captionText.text = _isControllerActive
                ? slide.controllerCaption
                : slide.keyboardCaption;

        // ✅ Update shared jump caption — hide if empty
        if (jumpCaptionText != null)
        {
            string jumpText = _isControllerActive
                ? slide.controllerJumpCaption
                : slide.keyboardJumpCaption;

            jumpCaptionText.text = jumpText;
            jumpCaptionText.gameObject.SetActive(!string.IsNullOrEmpty(jumpText));
        }
    }

    void LoadSlide(int index)
    {
        if (index < 0 || index >= slides.Count) return;

        _currentIndex = index;
        RefreshCaptions();

        videoPlayer.Stop();
        videoPlayer.clip = slides[index].videoClip;
        videoPlayer.isLooping = true;
        videoPlayer.Play();

        prevButton.interactable = index > 0;
        nextButton.interactable = index < slides.Count - 1;
    }

    public void NextSlide()
    {
        if (_currentIndex < slides.Count - 1)
            LoadSlide(_currentIndex + 1);
    }

    public void PreviousSlide()
    {
        if (_currentIndex > 0)
            LoadSlide(_currentIndex - 1);
    }

    public void ResetCarousel()
    {
        LoadSlide(0);
    }

    public void StopCarousel()
    {
        videoPlayer.Stop();
    }
}