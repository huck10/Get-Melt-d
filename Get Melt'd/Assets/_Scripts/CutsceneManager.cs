using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] VideoPlayer videoPlayer;
    [SerializeField] string sceneToLoadAfter = "Main";

    private static bool hasPlayedCutscene = false;

    private void Awake()
    {
        // 1. Start preparing the video as soon as the object exists
        if (!hasPlayedCutscene && videoPlayer != null)
        {
            videoPlayer.Prepare();
        }
    }

    private void Start()
    {
        if (hasPlayedCutscene)
        {
            LoadNextScene();
            return;
        }

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 2. We wait until the video is actually ready before playing
        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.loopPointReached += OnCutsceneFinished;

        // If it was already prepared in Awake, this will trigger immediately
        if (videoPlayer.isPrepared)
        {
            videoPlayer.Play();
        }
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        vp.Play();
    }

    private void Update()
    {
        if (Input.anyKeyDown)
        {
            videoPlayer.prepareCompleted -= OnVideoPrepared;
            videoPlayer.loopPointReached -= OnCutsceneFinished;
            hasPlayedCutscene = true;
            LoadNextScene();
        }
    }

    private void OnCutsceneFinished(VideoPlayer vp)
    {
        hasPlayedCutscene = true;
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(sceneToLoadAfter);
    }
}