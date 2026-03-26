using UnityEngine;

public class PauseGame : MonoBehaviour
{
    [SerializeField] GameStateManager gameStateManager;

    private void Start()
    {
        if (gameStateManager == null)
            Debug.LogError("PauseGame: GameStateManager is not assigned in Inspector!");
    }

    private void Update()
    {
        bool pausePressed = Input.GetKeyDown(KeyCode.Escape)
                         || Input.GetKeyDown(KeyCode.Joystick1Button7);

        if (pausePressed)
        {
            Debug.Log("Pause button pressed!");
            if (gameStateManager == null) return;
            gameStateManager.TogglePause();
        }
    }
}