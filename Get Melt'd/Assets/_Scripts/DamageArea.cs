using UnityEngine;

public class DamageArea : MonoBehaviour
{
    [Header("Damage Settings")]
    public float damageAmount = 0.1f;
    public float damageTickRate = 0.5f;

    private float nextDamageTime;

    private void OnCollisionStay(Collision collision)
    {
        if (Time.time >= nextDamageTime)
        {
            PlayerHealth player = collision.collider.GetComponentInParent<PlayerHealth>();

            if (player != null)
            {
                player.TakeDamage(damageAmount);

                nextDamageTime = Time.time + damageTickRate;
                Debug.Log("Sizzle! Player is melting. Next tick in " + damageTickRate + "s");
            }
        }
    }
}