using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingingObject : MonoBehaviour
{
    public float speed = 5f;
    public float targetAngle = 20f;

    void Update()
    {
        float currentX = transform.eulerAngles.x;
        float diff = Mathf.DeltaAngle(currentX, targetAngle);

        if (Mathf.Abs(diff) < 0.5f)
        {
            targetAngle *= -1f;
        }

        Quaternion target = Quaternion.Euler(targetAngle, 0f, 0f);
        Quaternion newRot = Quaternion.Lerp(transform.rotation, target, Time.deltaTime * speed);

        transform.rotation = newRot;
    }
}
