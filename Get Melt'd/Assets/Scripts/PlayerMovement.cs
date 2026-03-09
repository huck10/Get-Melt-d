using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public LayerMask groundMask;
    public Transform groundCheck;
    public float groundDistance = 0.5f;
    public float speed = 6.0f;
    public float rotationSpeed = 10.0f;

    [Header("Jump Settings")]
    public float maxJumpForce = 12.0f;
    public float minJumpForce = 8.0f;
    private float currentJumpForce;

    [Header("Physics & Falling")]
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    [Header("Weight Settings")]
    public float maxMass = 1.0f;
    public float minMass = 0.2f;

    private Rigidbody body;
    private bool isGrounded;
    private PlayerHealth healthScript;
    private Transform mainCameraTransform;

    void Start()
    {
        body = GetComponent<Rigidbody>();
        healthScript = GetComponent<PlayerHealth>();

        if (Camera.main != null)
            mainCameraTransform = Camera.main.transform;

        if (body != null)
        {
            body.freezeRotation = true;
            body.useGravity = true;
            body.mass = maxMass;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
    }

    void Update()
    {
        if (groundCheck != null)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
            Debug.DrawRay(groundCheck.position, Vector3.down * groundDistance, isGrounded ? Color.green : Color.red);
        }

        UpdatePhysicsStats();

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            body.velocity = new Vector3(body.velocity.x, 0, body.velocity.z);
            body.AddForce(Vector3.up * currentJumpForce, ForceMode.Impulse);
        }
    }

    void UpdatePhysicsStats()
    {
        if (healthScript != null && body != null && healthScript.maxHealth > 0)
        {
            float healthPercent = Mathf.Clamp01(healthScript.currentHealth / healthScript.maxHealth);
            body.mass = Mathf.Lerp(minMass, maxMass, healthPercent);
            currentJumpForce = Mathf.Lerp(maxJumpForce, minJumpForce, healthPercent);
        }
    }

    void FixedUpdate()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        if (mainCameraTransform == null) return;

        // Camera-relative direction logic
        Vector3 camForward = mainCameraTransform.forward;
        Vector3 camRight = mainCameraTransform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 movementDirection = (camForward * moveZ + camRight * moveX).normalized;

        if (movementDirection.magnitude >= 0.1f)
        {
            body.velocity = new Vector3(movementDirection.x * speed, body.velocity.y, movementDirection.z * speed);
            Quaternion targetRotation = Quaternion.LookRotation(movementDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
        else
        {
            body.velocity = new Vector3(0, body.velocity.y, 0);
        }

        ApplyCustomGravity();
    }

    void ApplyCustomGravity()
    {
        if (body.velocity.y < 0)
        {
            body.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (body.velocity.y > 0 && !Input.GetButton("Jump"))
        {
            body.velocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }
}