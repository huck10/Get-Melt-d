using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingPan : MonoBehaviour
{
    public float speed = 50f;
    private void Update()
    {
        transform.Rotate(0f, speed * Time.deltaTime, 0f);
    }
}
