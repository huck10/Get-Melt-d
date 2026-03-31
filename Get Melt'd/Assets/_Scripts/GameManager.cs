using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject pausePanel;

    [Header("References")]
    public GameStateManager stateManager;
    public ControllerCursor controllerCursor;

    private bool isPaused = false;
    private InputAction pauseAction;

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

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        if (isPaused) Resume();
        else Pause();
    }

    void Start()
    {
        Resume();
    }

    public void Pause()
    {
        isPaused = true;
        if (pausePanel != null) pausePanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (stateManager != null) stateManager.ChangeGameState(GameStates.PausedGame);
        if (controllerCursor != null) controllerCursor.SetCursorVisible(true);
    }

    public void Resume()
    {
        isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (stateManager != null) stateManager.ChangeGameState(GameStates.InGame);
        if (controllerCursor != null) controllerCursor.SetCursorVisible(false);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        if (stateManager != null) stateManager.ChangeGameState(GameStates.InGame);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        if (stateManager != null) stateManager.ChangeGameState(GameStates.InGame);
        SceneManager.LoadScene("MainMenu");
    }
}