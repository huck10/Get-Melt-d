using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] Transform visualModel;
    [SerializeField] ParticleSystem waterParticle;
    [SerializeField] GameObject skidPrefab;

    public float spawnDistance = 0.5f;
    public LayerMask groundMask;
    public LayerMask slopeMask;
    public Transform groundCheck;
    public float skidScaleOffset = 0.2f;

    public float groundDistance = 0.2f;
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
        {
            body.freezeRotation = true;
        }
    }

    void Update()
    {
        //Ground detector
        bool isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // Spawn particle ONLY when landing
        if (wasAirborne && isGrounded)
        {
            Instantiate(waterParticle, transform.position + Vector3.down * 0.5f, Quaternion.identity);
        }
        wasAirborne = !isGrounded;

        //Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            body.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        if (body.velocity.magnitude > 0.5f)
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
        //Movement
        MoveRelativeToCamera();
        DetectSlope();
    }

    void DetectSlope()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, Vector3.down, out hit, slopeDetectionDistance, slopeMask))
        {
            Quaternion slopeRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

            Quaternion finalRotation = slopeRotation * Quaternion.Euler(0, transform.eulerAngles.y, 0);

            visualModel.rotation = Quaternion.Slerp(visualModel.rotation, finalRotation, smooth * Time.deltaTime);
        }
    }

    void MoveRelativeToCamera()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Transform cam = Camera.main.transform;

        Vector3 forward = cam.forward;
        Vector3 right = cam.right;

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