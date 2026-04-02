using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject pausePanel;
    public GameObject winPanel;         // ✅ Assign your win panel here

    [Header("Win Settings")]
    public string winSceneName = "WinScene"; // ✅ Set the scene name that triggers win

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
        SceneManager.sceneLoaded += OnSceneLoaded; // ✅ Listen for scene changes
    }

    void OnDisable()
    {
        pauseAction.performed -= OnPausePerformed;
        pauseAction.Disable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        Resume();
        CheckForWinScene(); // ✅ Check immediately in case we start on the win scene
    }

    // ✅ Fires every time a scene loads
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckForWinScene();
    }

    void CheckForWinScene()
    {
        if (SceneManager.GetActiveScene().name == winSceneName)
        {
            ShowWinPanel();
        }
    }

    public void ShowWinPanel()
    {
        isPaused = true; // ✅ Prevent pausing while win panel is open
        if (winPanel != null) winPanel.SetActive(true);
        if (pausePanel != null) pausePanel.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (stateManager != null) stateManager.ChangeGameState(GameStates.PausedGame);
        if (controllerCursor != null) controllerCursor.SetCursorVisible(true);
        Debug.Log("🏆 Win panel shown!");
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        // ✅ Disable pause input while win panel is showing
        if (winPanel != null && winPanel.activeSelf) return;

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
        if (stateManager != null) stateManager.ChangeGameState(GameStates.PausedGame);
        if (controllerCursor != null) controllerCursor.SetCursorVisible(true);
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
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