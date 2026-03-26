using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health & Melting Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    [Tooltip("How much health/size is lost per second.")]
    public float meltRate = 2f;

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

    void Update()
    {
        // Continuous melting logic
        if (currentHealth > 0)
        {
            // Use TakeDamage so all the size and death logic stays centralized
            TakeDamage(meltRate * Time.deltaTime);
        }
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
        // Calculate percentage and apply to scale
        float healthPercent = currentHealth / maxHealth;
        float currentScaleMultiplier = Mathf.Lerp(minScale, 1f, healthPercent);
        transform.localScale = initialScale * currentScaleMultiplier;
    }

    public void Respawn()
    {
        currentHealth = maxHealth;
        UpdatePlayerSize(); // Reset scale immediately on respawn

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

        Debug.Log("The ice has melted! Respawning...");
    }

    public void SetNewRespawnAnchor(Transform newAnchor)
    {
        respawnAnchor = newAnchor;
    }

    // Optional: Call this to heal/refreeze the player (e.g., standing in a freezer)
    public void Refreeze(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdatePlayerSize();
    }
}