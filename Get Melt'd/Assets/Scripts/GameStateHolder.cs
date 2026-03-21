using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateHolder
{
    public static GameStates NextState = GameStates.InGame;
}

public enum GameStates
{
    InGame,
    InTutorial,
    PausedGame,
    LevelUp,
    EndGame
}

[System.Serializable]
public class GameStateTimeScale 
{
    public GameStates gameState;
    public float timeScale;
}
