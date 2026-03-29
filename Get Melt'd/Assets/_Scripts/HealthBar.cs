using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("References")]
    public Image healthFillImage;

    public void UpdateUI(float currentHealth, float maxHealth)
    {
        if (healthFillImage == null)
        {
            Debug.LogWarning("Health Fill Image is missing from HealthBar script!");
            return;
        }

        // Ensure we don't divide by zero
        if (maxHealth <= 0) return;

        // Fill Amount is 0.0 to 1.0
        float fillRatio = currentHealth / maxHealth;
        healthFillImage.fillAmount = fillRatio;
    }
}