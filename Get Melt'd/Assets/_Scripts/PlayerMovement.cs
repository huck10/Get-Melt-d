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

    // Fields matching ThirdPersonController style fallback
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool Grounded = true;
    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.14f;
    [Tooltip("The radius of the grounded check. Used when no groundCheck transform is assigned")]
    public float GroundedRadius = 0.28f;
    public LayerMask GroundLayers; // assign same as groundMask or use separately

    [Header("Movement Settings")]
    public float skidScaleOffset = 0.2f;
    public float spawnDistance = 0.5f;
    public float speed = 6.0f;
    public float jumpForce = 5.0f;

    private Rigidbody body;
    private float smooth = 10f;
    private float slopeDetectionDistance = 2.0f;
    private bool wasAirborne = true;
    private Vector3 lastSpawnPos;

    void Start()
    {
        body = GetComponent<Rigidbody>();
        if (body != null)
            body.freezeRotation = true;

        // If GroundLayers not set, default to groundMask for compatibility
        if (GroundLayers == 0 && groundMask != 0)
            GroundLayers = groundMask;
    }

    void Update()
    {
        // Use the new GroundedCheck method (box or sphere fallback)
        GroundedCheck();

        // CHANGED: Using CheckBox instead of CheckSphere for wider detection when groundCheck is assigned
        if (wasAirborne && Grounded)
            Instantiate(waterParticle, transform.position + Vector3.down * 0.5f, Quaternion.identity);

        wasAirborne = !Grounded;

        if (Input.GetButtonDown("Jump") && Grounded)
            body.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

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
        MoveRelativeToCamera();
        DetectSlope();
    }

    // Helps you see the ground checker in the Editor
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
            // draw fallback sphere
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
        }
    }

    // --- New GroundedCheck adapted from ThirdPersonController ---
    private void GroundedCheck()
    {
        bool grounded = false;

        if (groundCheck != null)
        {
            Vector3 halfExtents = boxSize * 0.5f;
            grounded = Physics.CheckBox(groundCheck.position, halfExtents, groundCheck.rotation, GroundLayers, QueryTriggerInteraction.Ignore);
        }
        else
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
            grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
        }

        Grounded = grounded;
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
        body.velocity = moveDir * speed + Vector3.up * body.velocity.y;

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
}
