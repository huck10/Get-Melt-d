using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceCube : MonoBehaviour
{
    [SerializeField] Transform spawnPoint;
    [SerializeField] GameObject meltedTextContainer;

    public float shrinkAmount = 0.1f;
    public float interval = 1f;
    public float intervalWhenTouchHot = 0.5f;
    public float showMeltedTextTime = 3.0f;

    private float originalInterval;
    private Vector3 originalScale;
    private float timer = 0f;
    private bool isMelted = false;
    private Rigidbody body;

    private void Start()
    {
        originalInterval = interval;
        originalScale = transform.localScale;
        body = GetComponent<Rigidbody>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= interval)
        {
            transform.localScale -= new Vector3(shrinkAmount, shrinkAmount, shrinkAmount);
            timer = 0f;

            if (transform.localScale.x < 0f)
            {
                transform.localScale = Vector3.zero;
                //isMelted = true;
                Respawn();
            }   
        }
    }

    public void Melt()
    {
        if (!isMelted)
        {
            interval = intervalWhenTouchHot;
        }
    }

    private void Respawn()
    {

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
}
