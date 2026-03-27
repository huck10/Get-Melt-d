using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] VideoPlayer videoPlayer;
    [SerializeField] string sceneToLoadAfter = "Main";

    // Static flag — persists during the session, resets when game is quit
    private static bool hasPlayedCutscene = false;

    private void Start()
    {
        if (hasPlayedCutscene)
        {
            // Already played this session — skip straight to Main
            LoadNextScene();
            return;
        }

        // Make sure game clock is running
        Time.timeScale = 1f;

        // Hide cursor during cutscene
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Play the video
        videoPlayer.loopPointReached += OnCutsceneFinished;
        videoPlayer.Play();
    }

    private void Update()
    {
        // Optional: skip cutscene by pressing any key or controller button
        if (Input.anyKeyDown)
        {
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