using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public GameStates CurrentGameState => currentGameState;
    public GameStateTimeScale[] gameStateTimeScale;
    private GameStates currentGameState;

    private void Start()
    {
        // Start with whatever state is next in your holder
        ChangeGameState(GameStateHolder.NextState);
    }

    public void ChangeGameState(GameStates newState)
    {
        foreach (var item in gameStateTimeScale)
        {
            if (item.gameState == newState)
            {
                currentGameState = newState;
                Time.timeScale = item.timeScale;
                Debug.LogWarning("GameState: " + currentGameState + " TimeScale: " + Time.timeScale);
                return;
            }
        }
        Time.timeScale = 1f;
    }
}