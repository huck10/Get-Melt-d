using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trail : MonoBehaviour
{
    public float duration = 2f;
    private Material mat;
    private float time = 0f;

    void Start()
    {
        mat = GetComponent<Renderer>().material;
    }

    void Update()
    {
        time += Time.deltaTime;
        float alpha = Mathf.Lerp(1f, 0f, time / duration);

        Color color = mat.color;
        color.a = alpha;
        mat.color = color;

        if(color.a <= 0f)
        {
            Destroy(gameObject, 1f);
        }
    }
}
