using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.UI.Image;

public class SceneLoader : MonoBehaviour
{
    public void PausedGame() { NextSceneGameState(GameStates.PausedGame); } //call this in buttons
    public void InGame() { NextSceneGameState(GameStates.InGame); }
    public void LevelUp() { NextSceneGameState(GameStates.LevelUp); }
    public void EndGame() { NextSceneGameState(GameStates.EndGame); }
    public void InTutorial() { NextSceneGameState(GameStates.InTutorial); }

    private void NextSceneGameState(GameStates newState)
    {
        GameStateHolder.NextState = newState;
    }

    public void LoadScene(string scene)
    {
        string sceneNoSpace = scene.Replace(" ", "");
        SceneManager.LoadScene(sceneNoSpace);
    }
}
