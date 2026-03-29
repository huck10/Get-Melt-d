using UnityEngine;

public class RotatingPan : MonoBehaviour
{
    public enum RotationDirection { Clockwise, CounterClockwise }

    [Header("Rotation")]
    public float speed = 50f;
    public RotationDirection direction = RotationDirection.Clockwise;
    public bool useLocalSpace = true; // rotate in local or world space

    private void Update()
    {
        float sign = (direction == RotationDirection.Clockwise) ? 1f : -1f;
        float angle = sign * speed * Time.deltaTime;

        if (useLocalSpace)
            transform.Rotate(0f, angle, 0f, Space.Self);
        else
            transform.Rotate(0f, angle, 0f, Space.World);
    }

    // Optional helper to flip direction from other scripts
    public void ToggleDirection()
    {
        direction = (direction == RotationDirection.Clockwise) ? RotationDirection.CounterClockwise : RotationDirection.Clockwise;
    }
    public Vector3 GetPointVelocity(Vector3 worldPoint)
    {
        Vector3 center = transform.position;
        Vector3 r = worldPoint - center;
        Vector3 rFlat = new Vector3(r.x, 0f, r.z);
        float angular = ((direction == RotationDirection.Clockwise) ? -1f : 1f) * speed * Mathf.Deg2Rad;
        Vector3 omega = new Vector3(0f, angular, 0f);
        Vector3 v = Vector3.Cross(omega, rFlat);
        return v;
    }

}
