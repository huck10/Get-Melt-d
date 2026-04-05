using System.Collections;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [Header("Movement Tutorial UI")]
    [SerializeField] GameObject keyboardMovementPrompt;
    [SerializeField] GameObject controllerMovementPrompt;

    [Header("Jump Tutorial UI")]
    [SerializeField] GameObject keyboardJumpPrompt;
    [SerializeField] GameObject controllerJumpPrompt;

    [Header("Jump Trigger")]
    [SerializeField] Transform jumpTriggerZone;
    [SerializeField] float jumpTriggerRadius = 2f;
    [SerializeField] Transform playerTransform;

    [Header("Settings")]
    [SerializeField] float movementHideDelay = 1.5f;

    private bool usingController = false;
    private bool movementDone = false;
    private bool jumpShown = false;
    private bool jumpDone = false;

    public void OnCinematicFinished()
    {
        DetectInputDevice();
        ShowMovementPrompt();
    }

    private void Update()
    {
        if (IsStrictControllerInput())
        {
            if (!usingController) { usingController = true; RefreshActivePrompts(); }
        }
        else if (IsStrictKeyboardInput())
        {
            if (usingController) { usingController = false; RefreshActivePrompts(); }
        }

        if (!movementDone && IsMoving())
            StartCoroutine(HideMovementAfterDelay());

        if (!jumpShown && !jumpDone && jumpTriggerZone != null && playerTransform != null)
        {
            float dist = Vector3.Distance(playerTransform.position, jumpTriggerZone.position);
            if (dist <= jumpTriggerRadius)
            {
                jumpShown = true;
                ShowJumpPrompt();
            }
        }

        if (jumpShown && !jumpDone && IsJumping())
            StartCoroutine(HideJumpAfterDelay());
    }

    // ── Show / Hide ──────────────────────────────────────────────

    private void ShowMovementPrompt()
    {
        SetActive(keyboardMovementPrompt, !usingController);
        SetActive(controllerMovementPrompt, usingController);
    }

    private IEnumerator HideMovementAfterDelay()
    {
        movementDone = true;
        yield return new WaitForSeconds(movementHideDelay);
        SetActive(keyboardMovementPrompt, false);
        SetActive(controllerMovementPrompt, false);
    }

    private void ShowJumpPrompt()
    {
        SetActive(keyboardJumpPrompt, !usingController);
        SetActive(controllerJumpPrompt, usingController);
    }

    private IEnumerator HideJumpAfterDelay()
    {
        jumpDone = true;
        yield return new WaitForSeconds(movementHideDelay);
        SetActive(keyboardJumpPrompt, false);
        SetActive(controllerJumpPrompt, false);
    }

    // ── UI helpers ───────────────────────────────────────────────

    private void RefreshActivePrompts()
    {
        if (!movementDone)
        {
            SetActive(keyboardMovementPrompt, !usingController);
            SetActive(controllerMovementPrompt, usingController);
        }

        if (jumpShown && !jumpDone)
        {
            SetActive(keyboardJumpPrompt, !usingController);
            SetActive(controllerJumpPrompt, usingController);
        }
    }

    private void DetectInputDevice()
    {
        usingController = false;

        string[] joysticks = Input.GetJoystickNames();
        foreach (string joystick in joysticks)
        {
            if (!string.IsNullOrWhiteSpace(joystick))
            {
                usingController = true;
                Debug.Log("🎮 Controller detected: " + joystick);
                break;
            }
        }

        if (!usingController)
            Debug.Log("⌨️ No controller detected — defaulting to keyboard.");
    }

    private static void SetActive(GameObject go, bool state)
    {
        if (go != null) go.SetActive(state);
    }

    // ── Input helpers ────────────────────────────────────────────

    private static bool IsMoving()
    {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D) ||
            Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) ||
            Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
            return true;

        // Controller left stick only via controller-specific axes
        float axis3 = 0f, axis4 = 0f;
        try { axis3 = Input.GetAxisRaw("3rd axis"); } catch { }
        try { axis4 = Input.GetAxisRaw("4th axis"); } catch { }
        if (Mathf.Abs(axis3) > 0.2f || Mathf.Abs(axis4) > 0.2f) return true;

        return false;
    }

    private static bool IsJumping()
    {
        if (Input.GetKeyDown(KeyCode.Space)) return true;
        if (Input.GetKeyDown(KeyCode.Joystick1Button0)) return true;
        return false;
    }

    private static bool IsStrictControllerInput()
    {
        for (int i = 0; i < 20; i++)
            if (Input.GetKeyDown(KeyCode.Joystick1Button0 + i)) return true;

        float axis3 = 0f, axis4 = 0f, axis5 = 0f, axis6 = 0f;
        try { axis3 = Input.GetAxisRaw("3rd axis"); } catch { }
        try { axis4 = Input.GetAxisRaw("4th axis"); } catch { }
        try { axis5 = Input.GetAxisRaw("5th axis"); } catch { }
        try { axis6 = Input.GetAxisRaw("6th axis"); } catch { }

        if (Mathf.Abs(axis3) > 0.5f || Mathf.Abs(axis4) > 0.5f ||
            Mathf.Abs(axis5) > 0.5f || Mathf.Abs(axis6) > 0.5f) return true;

        return false;
    }

    private static bool IsStrictKeyboardInput()
    {
        return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) ||
               Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D) ||
               Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) ||
               Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow) ||
               Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Return) ||
               Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ||
               Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ||
               Input.GetKey(KeyCode.Escape) || Input.GetKey(KeyCode.Tab) ||
               Input.GetKey(KeyCode.F) || Input.GetKey(KeyCode.E) ||
               Input.GetKey(KeyCode.R) || Input.GetKey(KeyCode.Q);
    }

    // ── Gizmos ───────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        if (jumpTriggerZone == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(jumpTriggerZone.position, jumpTriggerRadius);
    }
}