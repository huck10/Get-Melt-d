using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform visualModel;
    [SerializeField] ParticleSystem waterParticle;
    [SerializeField] GameObject skidPrefab;

    [Header("Ground Detection")]
    public LayerMask groundMask;
    public LayerMask slopeMask;
    public Transform groundCheck; // optional: used for box check (place at feet)
    public Vector3 boxSize = new Vector3(0.5f, 0.1f, 0.5f); // X = width, Y = thickness, Z = depth
    public float groundDistance = 0.2f;

    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool Grounded = true;
    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.14f;
    [Tooltip("The radius of the grounded check. Used when no groundCheck transform is assigned")]
    public float GroundedRadius = 0.28f;

    [Header("Movement Settings")]
    public float skidScaleOffset = 0.2f;
    public float spawnDistance = 0.5f;
    public float speed = 6.0f;

    [Header("Jump / Gravity Settings")]
    [Tooltip("Jump height in meters")]
    public float jumpHeight = 1.2f;
    [Tooltip("Custom gravity (negative). Use -9.81 or stronger for snappier fall")]
    public float gravity = -9.81f;
    [Tooltip("Maximum downward velocity")]
    public float terminalVelocity = -53f;
    [Tooltip("How long after leaving ground you can still jump (seconds)")]
    public float coyoteTime = 0.15f;
    [Tooltip("How long after pressing jump the input is buffered (seconds)")]
    public float jumpBufferTime = 0.12f;
    [Tooltip("Multiplier applied when releasing jump early (0..1). Lower = shorter hop")]
    [Range(0f, 1f)]
    public float jumpCutMultiplier = 0.5f;
    [Tooltip("Small threshold to detect apex (when upward velocity starts decreasing)")]
    public float apexThreshold = 0.01f;

    private Rigidbody body;
    private float smooth = 10f;
    private float slopeDetectionDistance = 2.0f;
    private bool wasAirborne = true;
    private Vector3 lastSpawnPos;

    // Jump / gravity runtime state
    private float verticalVelocity = 0f;
    private float coyoteTimer = 0f;
    private float jumpBufferTimer = 0f;
    private bool jumpRequested = false;
    private bool isApex = false;
    private float previousVerticalVelocity = 0f;

    [SerializeField] private VisualizationSettings visualization = new VisualizationSettings();
    private Collider _coll;

    void Start()
    {
        body = GetComponent<Rigidbody>();
        _coll = GetComponent<Collider>();
        if (body != null)
            body.freezeRotation = true;

        // ensure visualization uses the runtime groundMask if not set
        if (visualization.GroundLayer == 0 && groundMask != 0)
            visualization.GroundLayer = groundMask;
    }

    void Update()
    {
        // Read jump input and update jump buffer
        if (Input.GetButtonDown("Jump"))
        {
            jumpRequested = true;
            jumpBufferTimer = jumpBufferTime;
        }

        // Short hop: if player releases jump while ascending, shorten the jump
        if (Input.GetButtonUp("Jump"))
        {
            if (body.velocity.y > 0f)
            {
                Vector3 v = body.velocity;
                v.y *= jumpCutMultiplier;
                body.velocity = v;
            }
        }

        // decrement timers
        if (jumpBufferTimer > 0f) jumpBufferTimer -= Time.deltaTime;
        if (coyoteTimer > 0f) coyoteTimer -= Time.deltaTime;

        // Update grounded state
        GroundedCheck();

        // If grounded, reset coyote timer
        if (Grounded)
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

        // Attempt to consume jump buffer while within coyote time
        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            PerformJump();
            jumpBufferTimer = 0f;
            jumpRequested = false;
        }

        // spawn water particle on landing
        if (wasAirborne && Grounded)
            if (waterParticle != null)
                Instantiate(waterParticle, transform.position + Vector3.down * 0.5f, Quaternion.identity);

        wasAirborne = !Grounded;

        // other actions
        if (body.velocity.magnitude > 0.5f && Grounded)
        {
            if (Vector3.Distance(transform.position, lastSpawnPos) > spawnDistance)
            {
                SpawnSkid();
                lastSpawnPos = transform.position;
            }
        }
    }

    void FixedUpdate()
    {
        ApplyGravity();
        MoveRelativeToCamera();
        DetectSlope();
    }

    // Helps you see the ground checker in the Editor and draw arc
    void OnDrawGizmosSelected()
    {
        // ground check gizmo
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.matrix = groundCheck.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, boxSize);
        }
        else
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
        }

        // draw jump arc if enabled and collider exists
        if (visualization != null && visualization.DrawArc && _coll != null)
        {
            DrawJumpArc(visualization.HorizontalSpeed, visualization.ArcColor);
        }
    }

    // GroundedCheck now uses groundMask only
    private void GroundedCheck()
    {
        bool grounded = false;

        if (groundCheck != null)
        {
            Vector3 halfExtents = boxSize * 0.5f;
            grounded = Physics.CheckBox(groundCheck.position, halfExtents, groundCheck.rotation, groundMask, QueryTriggerInteraction.Ignore);
        }
        else
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
            grounded = Physics.CheckSphere(spherePosition, GroundedRadius, groundMask, QueryTriggerInteraction.Ignore);
        }

        Grounded = grounded;
    }

    // Apply gravity and detect apex/falling
    private void ApplyGravity()
    {
        if (body == null) return;

        // read current vertical velocity from rigidbody
        verticalVelocity = body.velocity.y;

        // apply custom gravity if not grounded
        if (!Grounded)
        {
            verticalVelocity += gravity * Time.fixedDeltaTime;
            verticalVelocity = Mathf.Max(verticalVelocity, terminalVelocity);
        }

        // apex detection: previous > 0 and current <= 0
        isApex = (previousVerticalVelocity > apexThreshold && verticalVelocity <= apexThreshold);

        // write back to rigidbody while preserving horizontal velocity
        Vector3 v = body.velocity;
        v.y = verticalVelocity;
        body.velocity = v;

        previousVerticalVelocity = verticalVelocity;
    }

    private void PerformJump()
    {
        if (body == null) return;

        // compute jump velocity needed to reach jumpHeight under our gravity
        float jumpVel = Mathf.Sqrt(Mathf.Max(0f, jumpHeight * -2f * gravity));

        Vector3 v = body.velocity;
        v.y = jumpVel;
        body.velocity = v;

        // reset timers
        coyoteTimer = 0f;
        jumpBufferTimer = 0f;
    }

    void DetectSlope()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, slopeDetectionDistance, slopeMask))
        {
            Quaternion slopeRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            Quaternion finalRotation = slopeRotation * Quaternion.Euler(0, transform.eulerAngles.y, 0);
            if (visualModel != null)
                visualModel.rotation = Quaternion.Slerp(visualModel.rotation, finalRotation, smooth * Time.deltaTime);
        }
    }

    void MoveRelativeToCamera()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Transform cam = Camera.main != null ? Camera.main.transform : null;
        Vector3 forward = cam != null ? cam.forward : Vector3.forward;
        Vector3 right = cam != null ? cam.right : Vector3.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDir = forward * moveZ + right * moveX;

        // preserve vertical velocity when setting rigidbody velocity
        float currentY = body != null ? body.velocity.y : 0f;
        if (body != null)
            body.velocity = moveDir * speed + Vector3.up * currentY;

        if (moveDir.magnitude > 0.1f)
        {
            float y = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, y, 0);
        }
    }

    void SpawnSkid()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f, groundMask))
        {
            Quaternion slopeRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            Vector3 offset = hit.normal * Random.Range(0.01f, 0.02f);
            GameObject skid = Instantiate(skidPrefab, hit.point + offset, slopeRotation);

            float playerWidthX = transform.localScale.x - skidScaleOffset;
            float playerWidthZ = transform.localScale.z - skidScaleOffset;
            skid.transform.localScale = new Vector3(playerWidthX, 1f, playerWidthZ);
        }
    }

    // Optional helpers for other scripts
    public bool IsApex() => isApex;
    public float GetVerticalVelocity() => verticalVelocity;

    // Draw jump arc using the same gravity and computed initial jump velocity
    private void DrawJumpArc(float moveSpeed, Color gizmoColor)
    {
        if (_coll == null) return;

        Vector3 startPosition = new Vector3(_coll.bounds.center.x, _coll.bounds.min.y, _coll.bounds.center.z);
        Vector3 previousPosition = startPosition;

        float horizontalSpeed = visualization.DrawRight ? moveSpeed : -moveSpeed;
        // compute initial vertical velocity from runtime jumpHeight & gravity so visualizer matches actual jump
        float initialJumpVel = Mathf.Sqrt(Mathf.Max(0f, jumpHeight * -2f * gravity));
        Vector3 velocity = new Vector3(horizontalSpeed, initialJumpVel, 0f);

        Gizmos.color = gizmoColor;

        float timeStep = (2f * visualization.TimeTillJumpApex) / Mathf.Max(1, visualization.ArcResolution);

        for (int i = 1; i <= visualization.VisualizationSteps; i++)
        {
            float simulationTime = i * timeStep;
            Vector3 displacement = Vector3.zero;

            if (simulationTime < visualization.TimeTillJumpApex) // ascending
            {
                float t = simulationTime;
                displacement = new Vector3(velocity.x * t, velocity.y * t + 0.5f * gravity * t * t, 0f);
            }
            else if (simulationTime < visualization.TimeTillJumpApex + visualization.ApexHangTime) // apex hang
            {
                float apexTime = simulationTime - visualization.TimeTillJumpApex;
                float ascendY = velocity.y * visualization.TimeTillJumpApex + 0.5f * gravity * visualization.TimeTillJumpApex * visualization.TimeTillJumpApex;
                displacement = new Vector3(velocity.x * visualization.TimeTillJumpApex + velocity.x * apexTime, ascendY, 0f);
            }
            else // descending
            {
                float descendTime = simulationTime - (visualization.TimeTillJumpApex + visualization.ApexHangTime);
                float ascendY = velocity.y * visualization.TimeTillJumpApex + 0.5f * gravity * visualization.TimeTillJumpApex * visualization.TimeTillJumpApex;
                float hangX = velocity.x * visualization.ApexHangTime;
                float descendY = 0.5f * gravity * descendTime * descendTime;
                displacement = new Vector3(velocity.x * visualization.TimeTillJumpApex + hangX + velocity.x * descendTime, ascendY + descendY, 0f);
            }

            // align horizontal displacement with player's forward direction
            Vector3 worldDisplacement = transform.TransformDirection(new Vector3(displacement.x, displacement.y, displacement.z));
            Vector3 drawPoint = startPosition + worldDisplacement;

            // collision check between previousPosition and drawPoint using groundMask
            if (visualization.StopOnCollision)
            {
                Vector3 dir = drawPoint - previousPosition;
                float dist = dir.magnitude;
                if (dist > 0.0001f)
                {
                    Ray ray = new Ray(previousPosition + Vector3.up * 0.01f, dir.normalized);
                    if (Physics.Raycast(ray, out RaycastHit hit, dist, groundMask, QueryTriggerInteraction.Ignore))
                    {
                        Gizmos.DrawLine(previousPosition, hit.point);
                        break;
                    }
                }
            }

            Gizmos.DrawLine(previousPosition, drawPoint);
            previousPosition = drawPoint;
        }
    }

    // Visualization-only settings (no runtime jump/ground data here)
    [System.Serializable]
    public class VisualizationSettings
    {
        public bool DrawArc = true;
        public bool DrawRight = true;                // horizontal direction
        public float HorizontalSpeed = 4f;           // horizontal speed used for visualization
        public float TimeTillJumpApex = 0.4f;        // time to apex (seconds) - used only for visualization step sizing
        public float ApexHangTime = 0.0f;            // optional hang time at apex
        public int ArcResolution = 30;               // how many time steps per half arc
        public int VisualizationSteps = 60;          // number of segments drawn
        public bool StopOnCollision = true;          // stop drawing when hitting ground/obstacle
        public LayerMask GroundLayer = 0;            // fallback; actual collision uses groundMask
        public Color ArcColor = Color.cyan;          // gizmo color
    }
}
