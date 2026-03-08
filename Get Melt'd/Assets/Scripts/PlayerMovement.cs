using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public LayerMask groundMask;
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public float speed = 6.0f;
    public float jumpForce = 5.0f;

    private Rigidbody body;
    private bool isGrounded;

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
        if (groundCheck != null)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        }

        //Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            body.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void FixedUpdate()
    {
        //Movement
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveX, 0, moveZ);

        body.velocity = new Vector3(movement.x * speed, body.velocity.y, movement.z * speed);
    }
}
