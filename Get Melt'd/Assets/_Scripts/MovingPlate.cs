using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlate : MonoBehaviour
{
    [SerializeField] Transform center;

    public float radius = 2f;
    public float speed = 10.0f;

    private Vector3 randomPoint;

    private void Start()
    {
        SetNewRandomPoint();
    }

    private void Update()
    {
        float distance = Vector3.Distance(transform.position, randomPoint);

        if (distance < 0.1f)
        {
            SetNewRandomPoint();
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            randomPoint,
            speed * Time.deltaTime
        );
    }

    void SetNewRandomPoint()
    {
        Vector2 random2D = Random.insideUnitCircle * radius;
        randomPoint = new Vector3(
            random2D.x + center.position.x,
            transform.position.y, // keep current height
            random2D.y + center.position.z
        );
    }

    private void OnDrawGizmos()
    {
        if (center == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center.position, radius);
    }
}
