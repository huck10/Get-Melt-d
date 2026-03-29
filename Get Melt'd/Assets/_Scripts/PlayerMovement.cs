using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform visualModel;
    [SerializeField] ParticleSystem waterParticle;
    [SerializeField] GameObject skidPrefab;

    [Header("Ground Detection")]
    public LayerMask groundMask;
    public LayerMask slopeMask;
    public Transform groundCheck; // place at feet
    public Vector3 boxSize = new Vector3(0.5f, 0.1f, 0.5f);
    public float GroundedRadius = 0.28f;
    public float GroundedOffset = -0.14f;

    [Header("Movement Settings")]
    public float speed = 6f;
    public float skidScaleOffset = 0.2f;
    public float spawnDistance = 0.5f;
    public LayerMask skidMask;

    [Header("Jump Settings")]
    public float jumpForce = 5f;
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.12f;
    [Range(0f, 1f)] public float jumpCutMultiplier = 0.5f;

    [Header("Slope / Visual")]
    public float slopeDetectionDistance = 2f;
    private float smooth = 10f;

    // runtime
    private Rigidbody body;
    private bool isGrounded = false;
    private bool wasAirborne = false;
    private Vector3 lastSpawnPos;
    private float coyoteTimer = 0f;
    private float jumpBufferTimer = 0f;
    private bool jumpRequested = false;

    void Start()
    {
        body = GetComponent<Rigidbody>();
        body.freezeRotation = true;
        body.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Update()
    {
        // Ground check (sphere at groundCheck or fallback)
        if (groundCheck != null)
            isGrounded = Physics.CheckSphere(groundCheck.position, GroundedRadius, groundMask, QueryTriggerInteraction.Ignore);
        else
        {
            Vector3 spherePos = new Vector3(transform.position.x, transform.position.y + GroundedOffset, transform.position.z);
            isGrounded = Physics.CheckSphere(spherePos, GroundedRadius, groundMask, QueryTriggerInteraction.Ignore);
        }

        // Water particle on landing
        ShowWaterParticle();

        // Jump input + buffer
        if (Input.GetButtonDown("Jump"))
        {
            jumpRequested = true;
            jumpBufferTimer = jumpBufferTime;
        }

        // Short hop (jump cut)
        if (Input.GetButtonUp("Jump"))
        {
            if (body.velocity.y > 0f)
            {
                Vector3 v = body.velocity;
                v.y *= jumpCutMultiplier;
                body.velocity = v;
            }
        }

        // timers
        if (coyoteTimer > 0f) coyoteTimer -= Time.deltaTime;
        if (jumpBufferTimer > 0f) jumpBufferTimer -= Time.deltaTime;

        // reset coyote when grounded
        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
            // small downward velocity to keep contact stable
            if (body.velocity.y < 0f)
            {
                Vector3 v = body.velocity;
                v.y = -2f;
                body.velocity = v;
            }
        }

        // consume jump buffer if within coyote
        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            DoJump();
            jumpBufferTimer = 0f;
            jumpRequested = false;
        }

        // spawn skid logic (keeps lastSpawnPos updated here so it uses current transform)
        SpawnSkid();
    }

    void FixedUpdate()
    {
        MoveRelativeToCamera();
        DetectSlope();
    }

    private void ShowWaterParticle()
    {
        if (wasAirborne && isGrounded)
        {
            if (waterParticle != null)
                Instantiate(waterParticle, transform.position + Vector3.down * 0.5f, Quaternion.identity);
        }
        wasAirborne = !isGrounded;
    }

    private void DoJump()
    {
        if (body == null) return;
        if (!isGrounded && coyoteTimer <= 0f) return;

        // impulse jump
        body.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        // reset coyote
        coyoteTimer = 0f;
    }

    void MoveRelativeToCamera()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Transform cam = Camera.main != null ? Camera.main.transform : null;
        Vector3 forward = cam != null ? cam.forward : Vector3.forward;
        Vector3 right = cam != null ? cam.right : Vector3.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDir = forward * moveZ + right * moveX;

        // preserve vertical velocity
        float currentY = body != null ? body.velocity.y : 0f;
        if (body != null)
            body.velocity = moveDir * speed + Vector3.up * currentY;

        if (moveDir.magnitude > 0.1f)
        {
            float y = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, y, 0f);
        }
    }

    void DetectSlope()
    {
        RaycastHit hit;
        LayerMask maskToUse = slopeMask != 0 ? slopeMask : groundMask;

        Vector3 origin;
        if (groundCheck != null)
            origin = groundCheck.position + Vector3.up * 0.1f;
        else
            origin = transform.position + Vector3.up * 0.1f;

        if (Physics.Raycast(origin, Vector3.down, out hit, slopeDetectionDistance, maskToUse, QueryTriggerInteraction.Ignore))
        {
            Quaternion slopeRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            Quaternion finalRotation = slopeRotation * Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
            if (visualModel != null)
                visualModel.rotation = Quaternion.Slerp(visualModel.rotation, finalRotation, smooth * Time.deltaTime);
        }
    }

    void SpawnSkid()
    {
        if (body == null) return;

        if (body.velocity.magnitude > 0.5f && isGrounded)
        {
            if (Vector3.Distance(transform.position, lastSpawnPos) > spawnDistance)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, Vector3.down, out hit, 4f, skidMask, QueryTriggerInteraction.Ignore))
                {
                    Quaternion slopeRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                    Vector3 offset = hit.normal * Random.Range(0.01f, 0.02f);
                    GameObject skid = Instantiate(skidPrefab, hit.point + offset, slopeRotation);

                    float playerWidthX = transform.localScale.x - skidScaleOffset;
                    float playerWidthZ = transform.localScale.z - skidScaleOffset;
                    skid.transform.localScale = new Vector3(playerWidthX, 1f, playerWidthZ);
                }
                lastSpawnPos = transform.position;
            }
        }
    }

    // Optional: visualize ground check in editor
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.matrix = groundCheck.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, boxSize);
        }
        else
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(new Vector3(transform.position.x, transform.position.y + GroundedOffset, transform.position.z), GroundedRadius);
        }
    }
}
