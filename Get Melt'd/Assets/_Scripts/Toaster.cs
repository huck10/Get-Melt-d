using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toaster : MonoBehaviour
{
    public Vector3 upLocalPosition;
    public Vector3 downLocalPosition;
    public float moveSpeed = 2f;
    public float startingInterval = 1.0f;
    public float minInterval = 1.0f;
    public float maxInterval = 3.0f;
    public float breadForce = 6.0f;

    private Rigidbody rb;
    private bool isUp = false;
    private float timer;
    private float randomInterval;
    private Vector3 targetLocal;
    private Vector3 previousPosition;


    void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
        rb.isKinematic = true;

        randomInterval = Random.Range(minInterval, maxInterval);
    }

    void Update()
    {
        targetLocal = isUp ? upLocalPosition : downLocalPosition;
        float distance = Vector3.Distance(transform.localPosition, targetLocal);
        if (distance < 0.01f)
        {
            timer += Time.deltaTime;
        }

        if (timer >= randomInterval)
        {
            isUp = !isUp;
            timer = 0;
            randomInterval = Random.Range(minInterval, maxInterval);
        }
    }

    void FixedUpdate()
    {
        previousPosition = rb.position;
        Vector3 targetWorld = rb.transform.position + (targetLocal - transform.localPosition);
        rb.MovePosition(Vector3.MoveTowards(rb.position, targetWorld, moveSpeed * Time.fixedDeltaTime));
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isUp) return;

        PlayerMovement player = collision.gameObject.GetComponentInParent<PlayerMovement>();
        if (player == null)
        {
            return;
        }
           
        Rigidbody body = player.GetComponentInParent<Rigidbody>();
        if (body == null)
        {
            return;
        }
           
        Vector3 breadVelocity = (rb.position - previousPosition) / Time.fixedDeltaTime;
        body.velocity = breadVelocity * breadForce;
    }
}
