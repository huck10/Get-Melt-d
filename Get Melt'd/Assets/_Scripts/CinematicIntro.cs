using System.Collections;
using UnityEngine;

public class CinematicIntro : MonoBehaviour
{
    // Static variable persists across scene reloads
    public static bool skipOnRestart = false;

    [Header("Camera Reference")]
    public Camera mainCamera;

    [Header("Cinematic Targets")]
    public Transform playerTransform;
    public Transform environmentCenter;
    public Transform goalTransform;

    [Header("Timing")]
    public float zoomOutDuration = 2.5f;
    public float travelToMiddleDuration = 2f;
    public float rotateDuration = 4f;
    public float travelToGoalDuration = 2f;
    public float holdGoalDuration = 1.5f;
    public float returnToPlayerDuration = 2f;
    public float holdPlayerDuration = 1f;

    [Header("Camera Settings")]
    public float zoomOutDistance = 8f;
    public float environmentHeight = 6f;
    public float rotateRadius = 5f;
    public float goalViewDistance = 4f;

    [Header("Post Cinematic")]
    public GameObject playerHUD;
    public MonoBehaviour playerController;

    [Header("Skip Prompts (UI GameObjects)")]
    public GameObject keyboardPromptObject;
    public GameObject controllerPromptObject;

    [Header("Disable During Cinematic")]
    public MonoBehaviour[] cameraScriptsToDisable;

    [Header("Tutorial")]
    public TutorialManager tutorialManager;

    private Transform _originalParent;
    private Vector3 _originalLocalPosition;
    private Quaternion _originalLocalRotation;

    private Coroutine _cinematicRoutine;
    private bool _isSkipping = false;
    private bool _isUsingController = false;

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        // 1. ALWAYS capture original positions first
        _originalParent = mainCamera.transform.parent;
        _originalLocalPosition = mainCamera.transform.localPosition;
        _originalLocalRotation = mainCamera.transform.localRotation;

        // 2. CHECK FOR RESTART SKIP
        if (skipOnRestart)
        {
            Debug.Log("⏩ Restart detected: Skipping Cinematic Sequence.");
            EndCinematic();
            return;
        }

        // 3. Normal Cinematic Setup
        mainCamera.transform.SetParent(null);

        foreach (var script in cameraScriptsToDisable)
            if (script != null) script.enabled = false;

        if (playerController != null) playerController.enabled = false;
        if (playerHUD != null) playerHUD.SetActive(false);

        UpdateInputDevice();
        UpdatePromptUI();

        _cinematicRoutine = StartCoroutine(PlayCinematic());
    }

    void Update()
    {
        DetectDeviceChange();

        if (!_isSkipping && (Input.anyKeyDown || Input.GetKeyDown(KeyCode.JoystickButton7)))
        {
            HandleSkip();
        }
    }

    private void DetectDeviceChange()
    {
        bool controllerConnected = Input.GetJoystickNames().Length > 0 && !string.IsNullOrEmpty(Input.GetJoystickNames()[0]);
        if (controllerConnected != _isUsingController)
        {
            _isUsingController = controllerConnected;
            UpdatePromptUI();
        }
    }

    private void UpdateInputDevice()
    {
        _isUsingController = Input.GetJoystickNames().Length > 0 && !string.IsNullOrEmpty(Input.GetJoystickNames()[0]);
    }

    private void UpdatePromptUI()
    {
        if (keyboardPromptObject != null) keyboardPromptObject.SetActive(!_isUsingController);
        if (controllerPromptObject != null) controllerPromptObject.SetActive(_isUsingController);
    }

    private void HandleSkip()
    {
        _isSkipping = true;
        if (keyboardPromptObject != null) keyboardPromptObject.SetActive(false);
        if (controllerPromptObject != null) controllerPromptObject.SetActive(false);

        if (_cinematicRoutine != null) StopCoroutine(_cinematicRoutine);
        StopAllCoroutines();

        EndCinematic();
    }

    IEnumerator PlayCinematic()
    {
        // Phase 1: Zoom out
        Vector3 zoomOutPos = playerTransform.position + Vector3.up * 3f - playerTransform.forward * zoomOutDistance;
        yield return StartCoroutine(MoveAndLook(zoomOutPos, playerTransform.position, zoomOutDuration));

        // Phase 2: Center
        Vector3 kitchenPos = environmentCenter.position + Vector3.up * environmentHeight + Vector3.back * rotateRadius;
        yield return StartCoroutine(MoveAndLook(kitchenPos, environmentCenter.position, travelToMiddleDuration));

        // Phase 3: Rotate
        yield return StartCoroutine(RotateAround(environmentCenter.position, rotateRadius, environmentHeight, rotateDuration));

        // Phase 4: Goal
        Vector3 goalPos = goalTransform.position - goalTransform.forward * goalViewDistance + Vector3.up * 2f;
        yield return StartCoroutine(MoveAndLook(goalPos, goalTransform.position, travelToGoalDuration));

        yield return new WaitForSeconds(holdGoalDuration);

        // Phase 6: Return
        Vector3 returnPos = playerTransform.position + Vector3.up * 2f - playerTransform.forward * 3f;
        yield return StartCoroutine(MoveAndLook(returnPos, playerTransform.position, returnToPlayerDuration));

        yield return new WaitForSeconds(holdPlayerDuration);

        EndCinematic();
    }

    IEnumerator MoveAndLook(Vector3 targetPos, Vector3 lookTarget, float duration)
    {
        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;
        Vector3 dir = (lookTarget - targetPos).normalized;
        Quaternion endRot = dir != Vector3.zero ? Quaternion.LookRotation(dir) : startRot;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }
        mainCamera.transform.position = targetPos;
        mainCamera.transform.rotation = endRot;
    }

    IEnumerator RotateAround(Vector3 center, float radius, float height, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float angle = 180f + 180f * t;
            float rad = angle * Mathf.Deg2Rad;
            mainCamera.transform.position = center + new Vector3(Mathf.Sin(rad) * radius, height, Mathf.Cos(rad) * radius);
            mainCamera.transform.LookAt(center);
            yield return null;
        }
    }

    void EndCinematic()
    {
        if (keyboardPromptObject != null) keyboardPromptObject.SetActive(false);
        if (controllerPromptObject != null) controllerPromptObject.SetActive(false);

        mainCamera.transform.SetParent(_originalParent);
        mainCamera.transform.localPosition = _originalLocalPosition;
        mainCamera.transform.localRotation = _originalLocalRotation;

        foreach (var script in cameraScriptsToDisable)
            if (script != null) script.enabled = true;

        if (playerController != null) playerController.enabled = true;
        if (playerHUD != null) playerHUD.SetActive(true);

        if (tutorialManager != null) tutorialManager.OnCinematicFinished();

        this.enabled = false;
    }
}