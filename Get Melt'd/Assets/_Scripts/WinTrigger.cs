using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WinTrigger : MonoBehaviour
{
    [Header("Transition Settings")]
    public Image transitionImage;
    public float fadeDuration = 1.0f;

    private GameManager gameManager;
    private bool hasWon = false;
    private bool canTrigger = false;

    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();

        if (transitionImage != null)
        {
            SetImageAlpha(0f);
            transitionImage.raycastTarget = false;
        }

        StartCoroutine(StartSafetyTimer());
    }

    private IEnumerator StartSafetyTimer()
    {
        yield return new WaitForSeconds(1.0f);
        canTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (canTrigger && other.CompareTag("Player") && !hasWon)
        {
            hasWon = true;
            StartCoroutine(PlayWinSequence());
        }
    }

    private IEnumerator PlayWinSequence()
    {
        if (transitionImage != null)
        {
            transitionImage.raycastTarget = true;

            // 1. FADE TO BLACK
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                SetImageAlpha(Mathf.Clamp01(elapsed / fadeDuration));
                yield return null;
            }
            SetImageAlpha(1f);
        }

        yield return new WaitForSecondsRealtime(0.5f);

        // 3. TRIGGER WIN PANEL
        if (gameManager != null)
        {
            gameManager.ShowWinPanel();

            Animator anim = gameManager.winPanel.GetComponentInChildren<Animator>();
            if (anim != null)
            {
                anim.Play("endcredits", -1, 0f);
            }
        }

        // 4. FADE BACK OUT
        if (transitionImage != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                SetImageAlpha(Mathf.Clamp01(1f - (elapsed / fadeDuration)));
                yield return null;
            }
            SetImageAlpha(0f);
            transitionImage.raycastTarget = false;
        }
    }

    private void SetImageAlpha(float alpha)
    {
        if (transitionImage == null) return;
        Color c = transitionImage.color;
        c.a = alpha;
        transitionImage.color = c;
    }
}