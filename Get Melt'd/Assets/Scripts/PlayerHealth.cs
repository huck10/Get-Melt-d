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

    [Header("UI Reference")]
    public HealthBar healthBarUI;

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

        // Force UI update on start
        UpdateUIReference();
    }

    void Update()
    {
        if (currentHealth > 0)
        {
            TakeDamage(meltRate * Time.deltaTime);
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdatePlayerSize();
        UpdateUIReference();

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

    // Helper method to ensure UI stays in sync
    void UpdateUIReference()
    {
        if (healthBarUI != null)
        {
            healthBarUI.UpdateUI(currentHealth, maxHealth);
        }
    }

    public void Respawn()
    {
        // 1. Reset Health Data
        currentHealth = maxHealth;

        // 2. Update Visuals immediately
        UpdatePlayerSize();
        UpdateUIReference();

        // 3. Reset Physics
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 4. Move to Spawn Point
        transform.position = (respawnAnchor != null) ? respawnAnchor.position : fallbackRespawnPoint;

        Debug.Log("The ice has melted! Respawning and resetting HP bar.");
    }

    public void Refreeze(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdatePlayerSize();
        UpdateUIReference();
    }
}