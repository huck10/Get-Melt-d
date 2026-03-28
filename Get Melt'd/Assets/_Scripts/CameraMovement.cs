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
    public float snapSpeed = 8f;

    [Header("Collision Settings")]
    public LayerMask collisionMask;           // environment layers only (exclude player)
    public float collisionPadding = 0.2f;
    public float cameraSphereRadius = 0.2f;
    public float collisionSmoothSpeed = 10f;  // higher = snappier

    private float currentX = 0f;
    private float currentY = 20f;
    private float currentDistance;
    private float desiredDistance;
    private bool isSnapping = false;
    private float snapTargetX = 0f;
    private float snapTargetY = 15f;

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

        float stickX = Input.GetAxis("RightStickX") * controllerSensitivity * Time.deltaTime * 60f;
        float stickY = Input.GetAxis("RightStickY") * controllerSensitivity * Time.deltaTime * 60f;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        if (Input.GetKeyDown(KeyCode.Joystick1Button9))
        {
            isSnapping = true;
            snapTargetX = target.eulerAngles.y;
            snapTargetY = 15f;
        }

        if (Mathf.Abs(stickX) > 0.1f || Mathf.Abs(stickY) > 0.1f)
            isSnapping = false;

        if (isSnapping)
        {
            currentX = Mathf.LerpAngle(currentX, snapTargetX, snapSpeed * Time.deltaTime);
            currentY = Mathf.Lerp(currentY, snapTargetY, snapSpeed * Time.deltaTime);

            if (Mathf.Abs(Mathf.DeltaAngle(currentX, snapTargetX)) < 0.5f &&
                Mathf.Abs(currentY - snapTargetY) < 0.5f)
            {
                currentX = snapTargetX;
                currentY = snapTargetY;
                isSnapping = false;
            }
        }
        else
        {
            currentX += mouseX + stickX;
            currentY -= mouseY + stickY;
            currentY = Mathf.Clamp(currentY, minVerticalAngle, maxVerticalAngle);
        }

        float scrollZoom = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        float triggerZoom = (Input.GetAxis("ZoomIn") - Input.GetAxis("ZoomOut")) * (zoomSpeed * 0.05f);

        if (Input.GetButtonDown("ZoomInBumper")) desiredDistance -= 0.5f;
        if (Input.GetButtonDown("ZoomOutBumper")) desiredDistance += 0.5f;

        desiredDistance -= scrollZoom + triggerZoom;
        desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
    }

    void LateUpdate()
    {
        if (!target) return;

        // rotation and desired positions
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 desiredPos = target.position + (rotation * offset) - (rotation * Vector3.forward * desiredDistance);

        // ray origin is the target eye (player + offset)
        Vector3 rayOrigin = target.position + (rotation * offset);
        Vector3 rayDir = (desiredPos - rayOrigin);
        float rayDist = rayDir.magnitude;
        if (rayDist <= 0.0001f) rayDir = -transform.forward; else rayDir /= rayDist;

        // SphereCast from eye toward desired camera position
        RaycastHit hit;
        float targetDistance = desiredDistance;

        if (Physics.SphereCast(rayOrigin, cameraSphereRadius, rayDir, out hit, rayDist, collisionMask, QueryTriggerInteraction.Ignore))
        {
            // place camera slightly in front of the hit point
            float hitDistance = Mathf.Max(hit.distance - collisionPadding, minDistance);
            targetDistance = Mathf.Clamp(hitDistance, minDistance, desiredDistance);
        }

        // smooth currentDistance toward targetDistance
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * collisionSmoothSpeed);

        // final camera position and look
        Vector3 finalPos = rayOrigin + rayDir * currentDistance;
        transform.position = finalPos;
        transform.LookAt(target.position + (rotation * Vector3.zero)); // look at target + offset center
    }
}
