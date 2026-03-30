using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewPlayerMovement : MonoBehaviour
{
    [SerializeField] Transform visualModel;
    [SerializeField] ParticleSystem waterParticle;
    [SerializeField] GameObject skidPrefab;

    [Header("Ground Detection")]
    public LayerMask groundMask;
    public LayerMask slopeMask;
    public LayerMask skidMask;
    public Transform groundCheck;
    public float groundDistance = 0.2f;

    [Header("Movement Settings")]
    public float skidScaleOffset = 0.2f;
    public float spawnDistance = 0.5f;
    public float speed = 6.0f;
    public float jumpForce = 5.0f;

    private PlayerHealth health;
    private Rigidbody body;
    private float smooth = 10f;
    private float slopeDetectionDistance = 2.0f;
    private bool wasAirborne = true;
    private Vector3 lastSpawnPos;
    private bool isGrounded;
    private HashSet<GameObject> touchingObjects = new HashSet<GameObject>();

    void Start()
    {
        health = GetComponent<PlayerHealth>();
        body = GetComponent<Rigidbody>();
        if (body != null)
            body.freezeRotation = true;
    }

    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, 1.0f, groundMask);
        ShowWaterParticle();
      
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            body.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        SpawnSkid();

        int count = touchingObjects.Count;
        if (count > 1)
        {
            Debug.Log("Currently touching " + count + " objects");
            health.TakeDamage(1000);
        }
    }

    void ShowWaterParticle()
    {
        if (wasAirborne && isGrounded)
        {
            Instantiate(waterParticle, transform.position + Vector3.down * 0.5f, Quaternion.identity);
        }
        wasAirborne = !isGrounded;
    }

    void FixedUpdate()
    {
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
        if (body.velocity.magnitude > 0.5f && isGrounded)
        {
            if (Vector3.Distance(transform.position, lastSpawnPos) > spawnDistance)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, Vector3.down, out hit, 4f, skidMask))
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

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Smash"))
            touchingObjects.Add(other.gameObject);
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("Smash"))
            touchingObjects.Remove(other.gameObject);
    }
}
