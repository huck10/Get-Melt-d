using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingMugs : MonoBehaviour
{
    public enum RotationAxis
    {
        X,
        Y,
        Z
    }
    public RotationAxis axis = RotationAxis.Y;
    public float speed = 5f;

    void Update()
    {
        if(axis == RotationAxis.X)
        {
            transform.Rotate(speed * Time.deltaTime, 0f, 0f);
        }
        else if(axis == RotationAxis.Y)
        {
            transform.Rotate(0f, speed * Time.deltaTime, 0f);
        }
        else
        {
            transform.Rotate(0f, 0f, speed * Time.deltaTime);
        }
    }
}
