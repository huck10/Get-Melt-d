using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoatatingMugs : MonoBehaviour
{
    public float speed = 5f;
    void Update()
    {
        transform.Rotate(0f, speed * Time.deltaTime, 0f);
    }
}
