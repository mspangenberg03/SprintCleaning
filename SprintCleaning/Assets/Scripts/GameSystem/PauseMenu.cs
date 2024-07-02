using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    private Level_Tracker _levelTracker => Level_Tracker.Instance;
    [SerializeField] private GameObject _generalUI;
    public void MainMenuButton()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene("MainMenu");
    }

    public void RestartButton()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene("Level " + _levelTracker._currentLevel);
    }

    public void ContinueButton() 
    {
        Time.timeScale = 1.0f;
        gameObject.SetActive(false);
        _generalUI.SetActive(true);
    }
}
