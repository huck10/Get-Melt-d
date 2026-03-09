using UnityEngine;

public class DamageArea : MonoBehaviour
{
    [Header("Damage Settings")]
    public float damageAmount = 10f;
    public float damageTickRate = 0.5f; // How many seconds between each "melt" tick

    private float nextDamageTime;

    // This runs every frame the player is touching the cube
    private void OnCollisionStay(Collision collision)
    {
        // Check if enough time has passed since the last damage tick
        if (Time.time >= nextDamageTime)
        {
            // Try to find the PlayerHealth script on the object touching us
            PlayerHealth player = collision.gameObject.GetComponent<PlayerHealth>();

            if (player != null)
            {
                player.TakeDamage(damageAmount);

                // Set the next time we are allowed to deal damage
                nextDamageTime = Time.time + damageTickRate;

                Debug.Log("Sizzle! Player is melting. Next tick in " + damageTickRate + "s");
            }
        }
    }
}