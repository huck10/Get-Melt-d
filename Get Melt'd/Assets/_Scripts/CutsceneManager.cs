using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    [Header("Video Settings")]
    [SerializeField] VideoPlayer videoPlayer;
    [SerializeField] string sceneToLoadAfter = "Main";

    [Header("Skip UI")]
    [SerializeField] GameObject keyboardSkipRoot;
    [SerializeField] GameObject controllerSkipRoot;

    private static bool hasPlayedCutscene = false;
    private bool usingController = false;

    private void Awake()
    {
        if (!hasPlayedCutscene && videoPlayer != null)
            videoPlayer.Prepare();
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

        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.loopPointReached += OnCutsceneFinished;

        if (videoPlayer.isPrepared)
            videoPlayer.Play();

        DetectInputDevice();
    }

    private void Update()
    {
        bool wasUsingController = usingController;

        if (IsAnyControllerButtonDown())
        {
            usingController = true;
            TriggerSkip();
            return;
        }

        if (Input.anyKeyDown)
        {
            usingController = false;
            TriggerSkip();
            return;
        }

        if (usingController != wasUsingController)
            RefreshSkipUI();
    }

    private void OnVideoPrepared(VideoPlayer vp) => vp.Play();

    private void OnCutsceneFinished(VideoPlayer vp)
    {
        hasPlayedCutscene = true;
        LoadNextScene();
    }

    private void TriggerSkip()
    {
        videoPlayer.prepareCompleted -= OnVideoPrepared;
        videoPlayer.loopPointReached -= OnCutsceneFinished;
        hasPlayedCutscene = true;
        LoadNextScene();
    }

    private void DetectInputDevice()
    {
        string[] joysticks = Input.GetJoystickNames();
        usingController = joysticks.Length > 0 && !string.IsNullOrEmpty(joysticks[0]);
        RefreshSkipUI();
    }

    private void RefreshSkipUI()
    {
        if (keyboardSkipRoot != null)
            keyboardSkipRoot.SetActive(!usingController);

        if (controllerSkipRoot != null)
            controllerSkipRoot.SetActive(usingController);
    }

    private bool IsAnyControllerButtonDown()
    {
        for (int i = 0; i < 20; i++)
        {
            if (Input.GetKeyDown(KeyCode.Joystick1Button0 + i))
                return true;
        }

        string[] axes = { "Horizontal", "Vertical", "3rd axis", "4th axis", "5th axis", "6th axis" };
        foreach (string axis in axes)
        {
            try
            {
                if (Mathf.Abs(Input.GetAxisRaw(axis)) > 0.5f)
                    return true;
            }
            catch { }
        }

        return false;
    }

    private void LoadNextScene()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(sceneToLoadAfter);
    }
}