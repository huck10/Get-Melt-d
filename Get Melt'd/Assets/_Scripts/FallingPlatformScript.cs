using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingPlatformScript : MonoBehaviour
{
    [Header("Timing Settings")]
    public float fallDelay = 1.0f;      
    public float respawnDelay = 3.0f;   

    [Header("Shake Settings")]
    public float shakeIntensity = 0.05f; 

    private Vector3 _startPosition;
    private Quaternion _startRotation;
    private Rigidbody _rb;
    private bool _isFalling = false;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _startPosition = transform.position;
        _startRotation = transform.rotation;

        _rb.isKinematic = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !_isFalling)
        {
            StartCoroutine(FallAndReset());
        }
    }

    IEnumerator FallAndReset()
    {
        _isFalling = true;

        // --- SHAKE LOGIC ---
        float elapsedTime = 0f;

        // Loop until the fall delay time is up
        while (elapsedTime < fallDelay)
        {
            // Move the platform slightly in a random direction based on its original spot
            transform.position = _startPosition + Random.insideUnitSphere * shakeIntensity;

            elapsedTime += Time.deltaTime;

            // Wait for the next frame before looping again
            yield return null;
        }

        
        transform.position = _startPosition;
        

        
        _rb.isKinematic = false;
        _rb.useGravity = true;

        
        yield return new WaitForSeconds(respawnDelay);

        
        ResetPlatform();
    }

    void ResetPlatform()
    {
        _isFalling = false;
        _rb.isKinematic = true;
        _rb.useGravity = false;
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        transform.position = _startPosition;
        transform.rotation = _startRotation;
    }
}
