using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraCollisionFollow : MonoBehaviour
{
    public Transform target;                // Player transform
    public Vector3 offset = new Vector3(0f, 1.5f, 0f);
    public float defaultDistance = 8f;
    public float minDistance = 1f;
    public float collisionPadding = 0.2f;   // how far from hit point the camera sits
    public LayerMask collisionMask;         // layers to collide with (include Ground, Walls)
    public float smoothSpeed = 10f;

    Vector3 currentVelocity;

    void LateUpdate()
    {
        if (target == null) return;

        // desired camera world position
        Vector3 desiredPos = target.position + offset - transform.forward * defaultDistance;

        // ray from target (eye) to desired camera position
        Vector3 rayOrigin = target.position + offset;
        Vector3 rayDir = (desiredPos - rayOrigin).normalized;
        float rayDist = defaultDistance;

        // check for collision
        if (Physics.SphereCast(rayOrigin, collisionPadding, rayDir, out RaycastHit hit, rayDist, collisionMask, QueryTriggerInteraction.Ignore))
        {
            // place camera slightly in front of the hit point
            float hitDistance = Mathf.Max(hit.distance - collisionPadding, minDistance);
            Vector3 newPos = rayOrigin + rayDir * hitDistance;
            transform.position = Vector3.SmoothDamp(transform.position, newPos, ref currentVelocity, 1f / smoothSpeed);
        }
        else
        {
            // no collision, move to desired position
            transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref currentVelocity, 1f / smoothSpeed);
        }

        // always look at the target
        transform.LookAt(target.position + offset);
    }
}
