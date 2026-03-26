using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] GameStateManager gameStateManager;

    [Header("Target Settings")]
    public Transform target;
    public Vector3 offset = new Vector3(0, 1.2f, 0);

    [Header("Distance Settings")]
    public float defaultDistance = 5.0f;
    public float minDistance = 0.5f; // Lowered slightly for tight spaces
    public float maxDistance = 8.0f;
    public float zoomSpeed = 4.0f;

    [Header("Rotation Settings")]
    public float mouseSensitivity = 3.0f;
    public float minVerticalAngle = -10f;
    public float maxVerticalAngle = 70f;

    [Header("Collision Settings")]
    public LayerMask collisionMask;
    public float collisionPadding = 0.2f;
    public float cameraSphereRadius = 0.2f; // Added radius to prevent clipping

    private float currentX = 0f;
    private float currentY = 20f;
    private float currentDistance;
    private float desiredDistance;

    void Start()
    {
        desiredDistance = defaultDistance;
        currentDistance = defaultDistance;

        // Ensure cursor is handled
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (gameStateManager != null && gameStateManager.CurrentGameState == GameStates.PausedGame)
        {
            return;
        }

        if (!target) return;

        // Input Handling
        currentX += Input.GetAxis("Mouse X") * mouseSensitivity;
        currentY -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        currentY = Mathf.Clamp(currentY, minVerticalAngle, maxVerticalAngle);

        desiredDistance -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
    }

    void LateUpdate()
    {
        if (!target) return;

        // Calculate Rotation and Base Position
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 targetPosition = target.position + (rotation * offset); // Offset now rotates with camera
        Vector3 direction = rotation * Vector3.back;

        // Robust Collision Check using SphereCast
        // This treats the camera like a physical ball so it can't poke through walls
        RaycastHit hit;
        if (Physics.SphereCast(targetPosition, cameraSphereRadius, direction, out hit, desiredDistance, collisionMask))
        {
            // If we hit something, snap the distance to the hit point minus padding
            currentDistance = Mathf.Clamp(hit.distance - collisionPadding, minDistance, desiredDistance);
        }
        else
        {
            // Smoothly zoom back out when no longer colliding
            currentDistance = Mathf.Lerp(currentDistance, desiredDistance, Time.deltaTime * 10f);
        }

        // Apply Position and Rotation
        transform.position = targetPosition + direction * currentDistance;
        transform.LookAt(targetPosition);
    }
}