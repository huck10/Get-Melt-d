using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Size Settings")]
    public float minScale = 0.3f;
    private Vector3 initialScale;

    [Header("Respawn Settings")]
    public Transform respawnAnchor;
    private Vector3 fallbackRespawnPoint;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentHealth = maxHealth;
        initialScale = transform.localScale;
        fallbackRespawnPoint = transform.position;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdatePlayerSize();

        if (currentHealth <= 0)
        {
            Respawn();
        }
    }

    void UpdatePlayerSize()
    {
        float healthPercent = currentHealth / maxHealth;
        float currentScaleMultiplier = Mathf.Lerp(minScale, 1f, healthPercent);
        transform.localScale = initialScale * currentScaleMultiplier;
    }

    public void Respawn()
    {
        currentHealth = maxHealth;
        transform.localScale = initialScale;

        // Stop all momentum so you don't keep falling/moving after respawning
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (respawnAnchor != null)
        {
            transform.position = respawnAnchor.position;
        }
        else
        {
            transform.position = fallbackRespawnPoint;
        }

        Debug.Log("Player Respawned!");
    }

    public void SetNewRespawnAnchor(Transform newAnchor)
    {
        respawnAnchor = newAnchor;
    }
}