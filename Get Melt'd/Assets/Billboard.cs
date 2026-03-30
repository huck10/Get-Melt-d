using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform camTransform;

    void Start()
    {
        // Automatically find the Main Camera
        if (Camera.main != null)
        {
            camTransform = Camera.main.transform;
        }
    }

    void LateUpdate()
    {
        if (camTransform != null)
        {
            // Make the sprite look at the camera
            transform.LookAt(transform.position + camTransform.forward);
        }
    }
}