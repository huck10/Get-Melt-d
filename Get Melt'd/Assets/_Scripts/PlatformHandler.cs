using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerPlatformHandler : MonoBehaviour
{
    private Rigidbody rb;
    private RotatingPan currentPan;
    private bool onPan = false;

    [Tooltip("How strongly the player inherits platform velocity (1 = full, 0 = none)")]
    [Range(0f, 1f)]
    public float inheritStrength = 1f;

    [Tooltip("Vertical offset from player's center to sample pan velocity (use feet height)")]
    public float sampleYOffset = -0.5f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    public void SetPlatform(RotatingPan pan)
    {
        currentPan = pan;
        onPan = pan != null;
    }

    public void ClearPlatform()
    {
        currentPan = null;
        onPan = false;
    }

    void FixedUpdate()
    {
        if (!onPan || currentPan == null) return;

        // sample a point near the player's feet to get correct tangential velocity
        Vector3 samplePoint = rb.worldCenterOfMass + Vector3.up * sampleYOffset;
        Vector3 panVel = currentPan.GetPointVelocity(samplePoint);

        // apply only horizontal components so we don't affect jumping/falling
        Vector3 v = rb.velocity;
        Vector3 horizontal = new Vector3(v.x, 0f, v.z);
        Vector3 targetHorizontal = horizontal + new Vector3(panVel.x, 0f, panVel.z) * inheritStrength;

        // preserve vertical velocity
        v.x = targetHorizontal.x;
        v.z = targetHorizontal.z;
        rb.velocity = v;
    }
}
