using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] GameStateManager gameStateManager;

    [Header("Target Settings")]
    public Transform target;
    public Vector3 offset = new Vector3(0, 1.2f, 0);

    [Header("Distance Settings")]
    public float defaultDistance = 5.0f;
    public float minDistance = 0.5f;
    public float maxDistance = 8.0f;
    public float zoomSpeed = 4.0f;

    [Header("Rotation Settings")]
    public float mouseSensitivity = 3.0f;
    public float controllerSensitivity = 4.0f;
    public float minVerticalAngle = -10f;
    public float maxVerticalAngle = 70f;

    [Header("Camera Snap Settings")]
    public float snapSpeed = 10f;  // how fast it snaps behind character

    [Header("Collision Settings")]
    public LayerMask collisionMask;
    public float collisionPadding = 0.2f;
    public float cameraSphereRadius = 0.2f;

    private float currentX = 0f;
    private float currentY = 20f;
    private float currentDistance;
    private float desiredDistance;
    private bool isSnapping = false;

    void Start()
    {
        desiredDistance = defaultDistance;
        currentDistance = defaultDistance;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (gameStateManager != null && gameStateManager.CurrentGameState == GameStates.PausedGame)
            return;

        if (!target) return;

        // Mouse look
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Right stick look (Xbox + PS)
        float stickX = Input.GetAxis("RightStickX") * controllerSensitivity * Time.deltaTime * 60f;
        float stickY = Input.GetAxis("RightStickY") * controllerSensitivity * Time.deltaTime * 60f;

        // If player moves the right stick, cancel any ongoing snap
        if (Mathf.Abs(stickX) > 0.1f || Mathf.Abs(stickY) > 0.1f)
            isSnapping = false;

        // RS click (joystick button 8) — snap camera behind character
        if (Input.GetKeyDown(KeyCode.Joystick1Button8))
            isSnapping = true;

        if (isSnapping)
        {
            // Smoothly rotate currentX toward character's facing direction
            float targetX = target.eulerAngles.y;
            float targetY = 20f; // reset to default vertical angle
            currentX = Mathf.LerpAngle(currentX, targetX, snapSpeed * Time.deltaTime);
            currentY = Mathf.Lerp(currentY, targetY, snapSpeed * Time.deltaTime);

            // Stop snapping once close enough
            if (Mathf.Abs(Mathf.DeltaAngle(currentX, targetX)) < 0.5f && Mathf.Abs(currentY - targetY) < 0.5f)
            {
                currentX = targetX;
                currentY = targetY;
                isSnapping = false;
            }
        }
        else
        {
            currentX += mouseX + stickX;
            currentY -= mouseY + stickY;
            currentY = Mathf.Clamp(currentY, minVerticalAngle, maxVerticalAngle);
        }

        // Mouse scroll zoom
        float scrollZoom = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;

        // RT/LT (Xbox) R2/L2 (PS) trigger zoom
        float triggerZoom = (Input.GetAxis("ZoomIn") - Input.GetAxis("ZoomOut")) * (zoomSpeed * 0.05f);

        // RB/LB (Xbox) R1/L1 (PS) bumper zoom — stepped
        if (Input.GetButtonDown("ZoomInBumper")) desiredDistance -= 0.5f;
        if (Input.GetButtonDown("ZoomOutBumper")) desiredDistance += 0.5f;

        desiredDistance -= scrollZoom + triggerZoom;
        desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
    }

    void LateUpdate()
    {
        if (!target) return;

        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 targetPosition = target.position + (rotation * offset);
        Vector3 direction = rotation * Vector3.back;

        RaycastHit hit;
        if (Physics.SphereCast(targetPosition, cameraSphereRadius, direction, out hit, desiredDistance, collisionMask))
            currentDistance = Mathf.Clamp(hit.distance - collisionPadding, minDistance, desiredDistance);
        else
            currentDistance = Mathf.Lerp(currentDistance, desiredDistance, Time.deltaTime * 10f);

        transform.position = targetPosition + direction * currentDistance;
        transform.LookAt(targetPosition);
    }
}