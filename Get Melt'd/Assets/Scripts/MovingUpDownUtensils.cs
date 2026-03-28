using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingUpDownUtensils : MonoBehaviour
{
    public float maxY;
    public float minY;
    public float speed = 2.0f;

    void Update()
    {
        transform.Translate(0, speed * Time.deltaTime, 0);

        if((transform.position.y >= maxY && speed > 0) || (transform.position.y <= minY && speed < 0))
        {
            speed *= -1f;
            Debug.Log("!");
        }
    }

}
