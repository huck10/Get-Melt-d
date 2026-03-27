using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirBlow : MonoBehaviour
{
    public float force = 10f;
    void OnTriggerStay(Collider other)
    {
        Rigidbody rb = other.GetComponentInParent<Rigidbody>();

        if (rb != null)
        {
            rb.AddForce(transform.up * force, ForceMode.Impulse); 
            Debug.Log("Object blown!");
        }
    }
}
