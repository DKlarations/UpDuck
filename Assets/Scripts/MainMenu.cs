using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    void Start()
    {

        
    }
    public void StartGame()
    {
        
        //Reset PlayerPrefs to game beginning
        PlayerPrefs.SetFloat("TimeElapsed", 0); 
        PlayerPrefs.SetFloat("PlayerXLocation", 54);
        PlayerPrefs.SetFloat("PlayerYLocation", -3); 

        for (int i = 1; i <= 4; i++)
        {
            PlayerPrefs.DeleteKey($"checkpoint_{i}");
        } 

        SceneManager.LoadScene(1);   // 1 is the Game
    }
    public void ResumeGame()
    {
        //This is likely a bandaid on a bigger problem, but could just be unique to the WebGL version:
        if (PlayerPrefs.GetFloat("TimeElapsed", -1) <= PlayerPrefs.GetFloat("checkpoint_1", -1))
        {
            for (int i = 1; i <= 4; i++)
            {
                PlayerPrefs.DeleteKey($"checkpoint_{i}");
            } 
        }
        //
        CheckpointManager.ResetCheckpointTimesLoaded();
        SceneManager.LoadScene(1);   // 1 is the Game
    }
    public void QuitGame()
    {
        Debug.Log("GAME QUIT");
        #if UNITY_EDITOR        
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

}
