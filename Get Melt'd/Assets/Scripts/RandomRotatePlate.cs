using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomRotatePlate : MonoBehaviour
{
    public float duration = 1f;
    public float chanceToRotate = 0.5f;

    private float elapsed = 0f;
    private bool rotating = false;

    private void Update()
    {
        if (!rotating) return;

        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, 360f, t));

        if (t >= 1f) rotating = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        IceCube ice = collision.gameObject.GetComponent<IceCube>();
        if(ice != null)
        {
            if (Random.value < chanceToRotate)
            {
                rotating = true;
                elapsed = 0f;
            }
        }
    }
}

