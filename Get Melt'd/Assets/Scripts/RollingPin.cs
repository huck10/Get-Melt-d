using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollingPin : MonoBehaviour
{
    public float torqueAmount = 10f;

    private Rigidbody otherObjRB;
    [SerializeField] private bool move = false;

    private void OnCollisionEnter(Collision collision)
    {
        IceCube ice = collision.gameObject.GetComponentInParent<IceCube>();
         
        if (ice != null)
        {
            move = true;
            otherObjRB = ice.GetComponent<Rigidbody>();
        }
    }

    void FixedUpdate()
    {
        if (move)
        {
            otherObjRB.AddTorque(transform.right * torqueAmount, ForceMode.Impulse);
        }
    }
}
