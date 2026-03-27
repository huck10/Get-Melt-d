using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollingPin : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody rb;
    private int direction = 1;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        rb.velocity = new Vector3(0f, rb.velocity.y, speed * direction);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("RollingPinStopper"))
        {
            direction *= -1;
        }
    }
}
