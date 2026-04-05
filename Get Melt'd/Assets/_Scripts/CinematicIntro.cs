using System.Collections;
using UnityEngine;

public class CinematicIntro : MonoBehaviour
{
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

    [Header("Disable During Cinematic")]
    public MonoBehaviour[] cameraScriptsToDisable;

    [Header("Tutorial")]
    public TutorialManager tutorialManager;

    private Transform _originalParent;
    private Vector3 _originalLocalPosition;
    private Quaternion _originalLocalRotation;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null) { Debug.LogError("❌ No camera found!"); return; }
        if (playerTransform == null) { Debug.LogError("❌ Player Transform not assigned!"); return; }
        if (environmentCenter == null) { Debug.LogError("❌ Environment Center not assigned!"); return; }
        if (goalTransform == null) { Debug.LogError("❌ Goal Transform not assigned!"); return; }

        // Save original parent and local transform before detaching
        _originalParent = mainCamera.transform.parent;
        _originalLocalPosition = mainCamera.transform.localPosition;
        _originalLocalRotation = mainCamera.transform.localRotation;

        // Detach camera from player so it moves freely
        mainCamera.transform.SetParent(null);
        Debug.Log("📷 Camera detached from: " + (_originalParent != null ? _originalParent.name : "none"));

        // Disable any scripts fighting for camera control
        foreach (var script in cameraScriptsToDisable)
        {
            if (script != null)
            {
                script.enabled = false;
                Debug.Log("⛔ Disabled: " + script.GetType().Name);
            }
        }

        if (playerController != null) playerController.enabled = false;
        if (playerHUD != null) playerHUD.SetActive(false);

        StartCoroutine(PlayCinematic());
    }

    IEnumerator PlayCinematic()
    {
        Debug.Log("🎬 Phase 1: Zoom out from player");

        // ── PHASE 1: Zoom out from player ──────────────────────────
        Vector3 zoomOutPos = playerTransform.position
                           + Vector3.up * 3f
                           - playerTransform.forward * zoomOutDistance;

        yield return StartCoroutine(MoveAndLook(
            zoomOutPos, playerTransform.position, zoomOutDuration));

        Debug.Log("🎬 Phase 2: Travel to environment center");

        // ── PHASE 2: Travel to environment center ──────────────────
        Vector3 kitchenPos = environmentCenter.position
                           + Vector3.up * environmentHeight
                           + Vector3.back * rotateRadius;

        yield return StartCoroutine(MoveAndLook(
            kitchenPos, environmentCenter.position, travelToMiddleDuration));

        Debug.Log("🎬 Phase 3: Rotate around environment");

        // ── PHASE 3: Slow rotation around environment ───────────────
        yield return StartCoroutine(RotateAround(
            environmentCenter.position, rotateRadius, environmentHeight, rotateDuration));

        Debug.Log("🎬 Phase 4: Travel to goal");

        // ── PHASE 4: Travel to goal ─────────────────────────────────
        Vector3 goalPos = goalTransform.position
                        - goalTransform.forward * goalViewDistance
                        + Vector3.up * 2f;

        yield return StartCoroutine(MoveAndLook(
            goalPos, goalTransform.position, travelToGoalDuration));

        Debug.Log("🎬 Phase 5: Hold on goal");

        // ── PHASE 5: Hold on goal ───────────────────────────────────
        yield return new WaitForSeconds(holdGoalDuration);

        Debug.Log("🎬 Phase 6: Return to player");

        // ── PHASE 6: Return to player ───────────────────────────────
        Vector3 returnPos = playerTransform.position
                          + Vector3.up * 2f
                          - playerTransform.forward * 3f;

        yield return StartCoroutine(MoveAndLook(
            returnPos, playerTransform.position, returnToPlayerDuration));

        Debug.Log("🎬 Phase 7: Hold then start game");

        // ── PHASE 7: Hold then hand control back ────────────────────
        yield return new WaitForSeconds(holdPlayerDuration);

        EndCinematic();
    }

    IEnumerator MoveAndLook(Vector3 targetPos, Vector3 lookTarget, float duration)
    {
        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;

        Vector3 dir = (lookTarget - targetPos).normalized;
        Quaternion endRot = dir != Vector3.zero
                                 ? Quaternion.LookRotation(dir)
                                 : startRot;

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
        float startAngle = 180f;
        float totalAngle = 180f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float angle = startAngle + totalAngle * t;
            float rad = angle * Mathf.Deg2Rad;

            mainCamera.transform.position = center + new Vector3(
                Mathf.Sin(rad) * radius,
                height,
                Mathf.Cos(rad) * radius
            );
            mainCamera.transform.LookAt(center);

            yield return null;
        }
    }

    void EndCinematic()
    {
        Debug.Log("🎬 Cinematic complete — restoring camera to player!");

        // Re-attach camera back to original parent (the player)
        mainCamera.transform.SetParent(_originalParent);
        mainCamera.transform.localPosition = _originalLocalPosition;
        mainCamera.transform.localRotation = _originalLocalRotation;

        // Re-enable camera scripts
        foreach (var script in cameraScriptsToDisable)
            if (script != null) script.enabled = true;

        if (playerController != null) playerController.enabled = true;
        if (playerHUD != null) playerHUD.SetActive(true);

        // Hand off to tutorial
        if (tutorialManager != null)
            tutorialManager.OnCinematicFinished();
        else
            Debug.LogWarning("⚠️ TutorialManager not assigned — tutorial won't trigger.");
    }
}