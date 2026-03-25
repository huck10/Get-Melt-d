using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceCube : MonoBehaviour
{
    public float shrinkAmount = 0.1f;
    public float interval = 1f;
    public float intervalWhenTouchHot = 0.5f;

    private float originalInterval;
    private Vector3 originalScale;
    private float timer = 0f;
    private bool isMelted = false;

    private void Start()
    {
        originalInterval = interval;
        originalScale = transform.localScale;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= interval)
        {
            transform.localScale -= new Vector3(shrinkAmount, shrinkAmount, shrinkAmount);
            timer = 0f;

            if (transform.localScale.x < 0f)
            {
                transform.localScale = Vector3.zero;
                isMelted = true;
            }   
        }
    }

    public void Melt()
    {
        if (!isMelted)
        {
            interval = intervalWhenTouchHot;
        }
    }
}
