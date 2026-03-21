using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    [SerializeField] GameObject pauseMenuContainer;

    public GameStates CurrentGameState => currentGameState;
    public GameStateTimeScale[] gameStateTimeScale; 

    private GameStates currentGameState; 
    private float currentTimeScale;

    public void PausedGame() { ChangeGameState(GameStates.PausedGame); } //call this in buttons
    public void InGame() { ChangeGameState(GameStates.InGame); }
    public void LevelUp() { ChangeGameState(GameStates.LevelUp); }
    public void EndGame() { ChangeGameState(GameStates.EndGame); }
    public void InTutorial() { ChangeGameState(GameStates.InTutorial); }

    private void Start()
    {
        ChangeGameState(GameStateHolder.NextState);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    private void ChangeGameState(GameStates newState) //change state and timescale automatically
    {
        foreach(var item in gameStateTimeScale)
        {
            if(item.gameState == newState)
            {
                currentGameState = newState;
                Time.timeScale = item.timeScale;
                currentTimeScale = item.timeScale;

                Debug.LogWarning("GameState: " + currentGameState + " TimeScale: " + currentTimeScale);
                return;
            }
        }

        Debug.LogWarning("GameState not found!");
        Time.timeScale = 1f;
    }

    public void TogglePause()
    {
        if(pauseMenuContainer == null)
        {
            Debug.Log("Pause menu container not found!");
            return;
        }

        if (currentGameState == GameStates.InGame)
        {
            PausedGame();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            pauseMenuContainer.SetActive(true);
        }
        else if (currentGameState == GameStates.PausedGame)
        {
            InGame();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            pauseMenuContainer.SetActive(false);
        }
    }

}


