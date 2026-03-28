using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlate : MonoBehaviour
{
    [SerializeField] Transform center;

    public float radius = 2f;
    public float speed = 10.0f;
    private Rigidbody rb;
    private Vector3 randomPoint;

    private bool arrived = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        Vector2 random2D = Random.insideUnitCircle * radius;
        randomPoint = new Vector3(random2D.x, 0f, random2D.y) + center.position;   
    }

    private void FixedUpdate()
    {
        float distance = Vector3.Distance(rb.position, randomPoint);

        if(distance < 1)
        {
            rb.velocity = Vector3.zero;
            arrived = true;
        }

        if (rb != null && !arrived)
        {
            rb.MovePosition(Vector3.MoveTowards(rb.position, randomPoint, speed * Time.fixedDeltaTime));
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center.position, radius);
    }
}
