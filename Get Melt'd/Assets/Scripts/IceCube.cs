using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class IceCube : MonoBehaviour
{
    [SerializeField] Transform spawnPoint;
    [SerializeField] GameObject meltedTextContainer;
    [SerializeField] TextMeshProUGUI hpTempText;

    public float shrinkAmount = 0.1f;
    public float interval = 1f;
    public float showMeltedTextTime = 3.0f;

    private float originalInterval;
    private Vector3 originalScale;
    private float timer = 0f;
    private bool isMelted = false;
    private Rigidbody body;
    private float hp = 100f;

    private void Start()
    {
        originalInterval = interval;
        originalScale = transform.localScale;
        body = GetComponent<Rigidbody>();
        hpTempText.text = "HP: " + Mathf.FloorToInt(hp);
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= interval)
        {
            Shrink(shrinkAmount);
            timer = 0;
        }
    }

    private void Respawn()
    {
        isMelted = false;
        if(spawnPoint == null)
        {
            Debug.Log("No respawn point");
            return;
        }

        if(meltedTextContainer == null)
        {
            Debug.Log("No melted text");
            return;
        }

        body.velocity = Vector3.zero;
        transform.position = spawnPoint.position;
        transform.localScale = originalScale;
        StartCoroutine(EnableDisable(meltedTextContainer));   
    }

    private IEnumerator EnableDisable(GameObject obj)
    {
        obj.SetActive(true);

        yield return new WaitForSeconds(showMeltedTextTime);

        obj.SetActive(false);
    }

    public void Shrink(float amount)
    {
        if (isMelted)
        {
            Debug.Log("Melted already!");
            return;
        }

        transform.localScale -= new Vector3(amount, amount, amount);
        transform.localScale = new Vector3(Mathf.Max(transform.localScale.x, 0f), Mathf.Max(transform.localScale.y, 0f), Mathf.Max(transform.localScale.z, 0f));
        hp = (transform.localScale.x / originalScale.x) * 100f;

        if (transform.localScale.x <= 0f && !isMelted)
        {
            isMelted = true;
            Respawn();
        }

        hpTempText.text = "HP: " + Mathf.FloorToInt(hp);
    }
}
