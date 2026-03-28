using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sponge : MonoBehaviour
{
    public float spongeForce = 6.0f;
    private void OnCollisionEnter(Collision collision)
    {
        PlayerMovement player = collision.gameObject.GetComponentInParent<PlayerMovement>();
        if (player == null)
        {
            return;
        }

        Rigidbody body = player.GetComponentInParent<Rigidbody>();
        if (body == null)
        {
            return;
        }
        body.velocity = Vector3.up * spongeForce;
    }
}
