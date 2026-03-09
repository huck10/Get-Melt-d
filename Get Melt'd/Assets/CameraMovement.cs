using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;
    public Vector3 offset = new Vector3(0, 1.2f, 0);

    [Header("Distance Settings")]
    public float defaultDistance = 5.0f;
    public float minDistance = 1.5f;
    public float maxDistance = 8.0f;
    public float zoomSpeed = 4.0f;

    [Header("Rotation Settings")]
    public float mouseSensitivity = 3.0f;
    public float minVerticalAngle = -10f;
    public float maxVerticalAngle = 70f;

    [Header("Collision Settings")]
    public LayerMask collisionMask;
    public float collisionPadding = 0.3f;

    private float currentX = 0f;
    private float currentY = 20f;
    private float currentDistance;
    private float desiredDistance;

    void Start()
    {
        desiredDistance = defaultDistance;
        currentDistance = defaultDistance;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!target) return;

        currentX += Input.GetAxis("Mouse X") * mouseSensitivity;
        currentY -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        currentY = Mathf.Clamp(currentY, minVerticalAngle, maxVerticalAngle);

        desiredDistance -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
    }

    void LateUpdate()
    {
        if (!target) return;

        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 targetPosition = target.position + offset;
        Vector3 direction = rotation * Vector3.back;

        // Collision Check
        RaycastHit hit;
        if (Physics.Raycast(targetPosition, direction, out hit, desiredDistance, collisionMask))
        {
            currentDistance = Mathf.Clamp(hit.distance - collisionPadding, minDistance, desiredDistance);
        }
        else
        {
            currentDistance = Mathf.Lerp(currentDistance, desiredDistance, Time.deltaTime * 5f);
        }

        transform.position = targetPosition + direction * currentDistance;
        transform.LookAt(targetPosition);
    }
}