using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject pausePanel;

    [Header("References")]
    public GameStateManager stateManager; // Drag your GameStateManager object here

    private bool isPaused = false;

    void Start()
    {
        // If stateManager is missing, try to find it in the scene
        if (stateManager == null) stateManager = FindFirstObjectByType<GameStateManager>();

        Resume();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    public void Pause()
    {
        isPaused = true;

        // Visuals
        if (pausePanel != null) pausePanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Logic
        if (stateManager != null) stateManager.ChangeGameState(GameStates.PausedGame);
    }

    public void Resume()
    {
        isPaused = false;

        // Visuals
        if (pausePanel != null) pausePanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Logic
        if (stateManager != null) stateManager.ChangeGameState(GameStates.InGame);
    }

    public void RestartGame()
    {
        // Important: Reset state to InGame before loading
        if (stateManager != null) stateManager.ChangeGameState(GameStates.InGame);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitToMainMenu()
    {
        if (stateManager != null) stateManager.ChangeGameState(GameStates.InGame);
        SceneManager.LoadScene("MainMenu");
    }
}