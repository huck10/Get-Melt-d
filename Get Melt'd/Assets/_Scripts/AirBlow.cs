using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirBlow : MonoBehaviour
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right,
        Forward,
        Back
    }
    public Direction direction = Direction.Up;

    public float force = 10f;
    void OnTriggerStay(Collider other)
    {
        Rigidbody rb = other.GetComponentInParent<Rigidbody>();

        if (rb != null)
        {
            if(direction == Direction.Up)
            {
                rb.AddForce(transform.up * force, ForceMode.Impulse);
            }
            else if (direction == Direction.Down)
            {
                rb.AddForce(-transform.up * force, ForceMode.Impulse);
            }
            else if(direction == Direction.Left)
            {

                rb.AddForce(-transform.right * force, ForceMode.Impulse);
            }
            else if(direction == Direction.Right)
            {

                rb.AddForce(transform.right * force, ForceMode.Impulse);
            }
            else if(direction == Direction.Forward)
            {
                rb.AddForce(transform.forward * force, ForceMode.Impulse);
            }
            else if(direction == Direction.Back)
            {
                rb.AddForce(-transform.forward * force, ForceMode.Impulse);
            }  
            Debug.Log("Object blown!");
        }
    }
}
