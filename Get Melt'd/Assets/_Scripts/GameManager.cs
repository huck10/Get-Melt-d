using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject pausePanel;
    public GameObject winPanel;

    [Header("References")]
    public GameStateManager stateManager;
    public ControllerCursor controllerCursor;

    private bool isPaused = false;
    private InputAction pauseAction;
    private bool creditsActive = false;

    void Awake()
    {
        if (stateManager == null) stateManager = FindFirstObjectByType<GameStateManager>();
        if (controllerCursor == null) controllerCursor = FindFirstObjectByType<ControllerCursor>();

        pauseAction = new InputAction("Pause", binding: "<Gamepad>/start");
        pauseAction.AddBinding("<Keyboard>/escape");
        pauseAction.AddBinding("<Keyboard>/p");
    }

    void OnEnable()
    {
        pauseAction.Enable();
        pauseAction.performed += OnPausePerformed;
    }

    void OnDisable()
    {
        pauseAction.performed -= OnPausePerformed;
        pauseAction.Disable();
    }

    void Start()
    {
        Resume();
    }

    public void ShowWinPanel()
    {
        isPaused = true;
        creditsActive = true;

        if (winPanel != null) winPanel.SetActive(true);
        if (pausePanel != null) pausePanel.SetActive(false);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (stateManager != null) stateManager.ChangeGameState(GameStates.PausedGame);
        if (controllerCursor != null) controllerCursor.SetCursorVisible(true);

        StartCoroutine(WaitAndReturnToMenu());
    }

    private IEnumerator WaitAndReturnToMenu()
    {
        Animator anim = winPanel.GetComponentInChildren<Animator>();
        if (anim != null)
        {
            yield return null;
            float animationLength = anim.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSecondsRealtime(animationLength + 1.0f);
            if (creditsActive) QuitToMainMenu();
        }
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        if (creditsActive)
        {
            QuitToMainMenu();
            return;
        }

        if (isPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        if (pausePanel != null) pausePanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (controllerCursor != null) controllerCursor.SetCursorVisible(true);
    }

    public void Resume()
    {
        isPaused = false;
        creditsActive = false;
        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (controllerCursor != null) controllerCursor.SetCursorVisible(false);
        if (stateManager != null) stateManager.ChangeGameState(GameStates.InGame);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        // SET THE FLAG TO SKIP CINEMATIC ON RELOAD
        CinematicIntro.skipOnRestart = true;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;

        // RESET THE FLAG SO CINEMATIC PLAYS NEXT TIME THEY START A FRESH GAME
        CinematicIntro.skipOnRestart = false;

        if (stateManager != null) stateManager.ChangeGameState(GameStates.InGame);
        SceneManager.LoadScene("MainMenu");
    }
}