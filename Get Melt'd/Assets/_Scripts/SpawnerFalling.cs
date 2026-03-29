using UnityEngine;

public class SimpleFallerSpawner : MonoBehaviour
{
    [Header("Spawn")]
    public Transform spawnPoint;
    public GameObject fallPrefab;
    public float spawnInterval = 1.0f;
    public float destroyAfterSeconds = 10f;
    public Vector3 randomOffset = new Vector3(0.5f, 0f, 0.5f); // randomize X/Z

    private float timer;

    void Update()
    {
        if (fallPrefab == null || spawnPoint == null) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            SpawnOne();
            timer = spawnInterval;
        }
    }

    void SpawnOne()
    {
        Vector3 pos = spawnPoint.position;
        pos.x += Random.Range(-randomOffset.x, randomOffset.x);
        pos.z += Random.Range(-randomOffset.z, randomOffset.z);
        pos.y += randomOffset.y; // if you want vertical offset

        GameObject go = Instantiate(fallPrefab, pos, Quaternion.identity);

        Destroy(go,destroyAfterSeconds);
        // ensure physics starts clean
        Rigidbody rb = go.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (spawnPoint == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(spawnPoint.position + new Vector3(0, 0, 0), new Vector3(randomOffset.x * 2f, 0.1f, randomOffset.z * 2f));
    }
}
