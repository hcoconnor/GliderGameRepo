using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{

    public GameObject pauseMenuObj;
    public playerControllerScript player;

    public bool canPause = true;
    public static bool isPaused;

    // Start is called before the first frame update
    void Start()
    {
        unPauseGame();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("updating");
        if (Input.GetButtonDown("Cancel") && canPause)
        {
            //Debug.Log("paused "+isPaused);
            if (!isPaused)
            {
                pauseGame();
            }
            else
            {
                unPauseGame();
            }
        }
    }

    public void pauseGame()
    {

        pauseMenuObj.SetActive(true);
        Time.timeScale = 0;
        isPaused = true;
    }

    public void unPauseGame()
    {
        pauseMenuObj.SetActive(false);
        Time.timeScale = 1;
        isPaused = false;
    }

    public void returnToMenu()
    {   
        //Debug.Log(SceneManager.GetSceneAt(1).name);
        SceneManager.LoadScene("StartMenu");
        
    }

    public void resetToCheckPoint()
    {
        player.resetToCheckPoint();
        unPauseGame();
    }

    public void reloadLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }



    public void nextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

}
