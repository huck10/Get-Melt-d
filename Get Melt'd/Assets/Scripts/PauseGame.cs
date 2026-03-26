using UnityEngine;

public class PauseGame : MonoBehaviour
{
    [SerializeField] GameStateManager gameStateManager;

    private void Start()
    {
        if (gameStateManager == null)
            Debug.Log("No GameStateManager!");
    }

    private void Update()
    {
        // joystick button 7 = Start/Menu (Xbox) / Options (PS)
        if (Input.GetKeyDown(KeyCode.Joystick1Button7))
        {
            if (gameStateManager == null) return;
            gameStateManager.TogglePause();
        }
    }
}