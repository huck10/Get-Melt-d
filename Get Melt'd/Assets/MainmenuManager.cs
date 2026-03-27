using UnityEngine;
using UnityEngine.SceneManagement;

public class MainmenuManager : MonoBehaviour
{
    private void Awake()
    {
        // This is the "Magic Fix"
        // It unfreezes the game clock so buttons can process clicks again
        Time.timeScale = 1f;
    }

    public void LoadSceneByName(string sceneName)
    {
        Debug.Log("Attempting to load: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}