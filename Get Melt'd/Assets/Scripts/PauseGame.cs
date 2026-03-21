using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseGame : MonoBehaviour
{
    [SerializeField] GameStateManager gameStateManager;
    [SerializeField] 

    private void Start()
    {
        if(gameStateManager == null)
        {
            Debug.Log("No GameStateManager!");
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
           
        }
    }
}
