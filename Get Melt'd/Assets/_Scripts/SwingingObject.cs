using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingingObject : MonoBehaviour
{
    public float speed = 5f;
    public float targetAngle = 20f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float currentZ = transform.eulerAngles.z;
        float diff = Mathf.DeltaAngle(currentZ, targetAngle);

        if (Mathf.Abs(diff) < 0.5f)
        {
            targetAngle *= -1f;
        }

        Quaternion target = Quaternion.Euler(0f, 0f, targetAngle);
        Quaternion newRot = Quaternion.Lerp(rb.rotation, target, Time.fixedDeltaTime * speed);

        rb.MoveRotation(newRot);
    }
}
