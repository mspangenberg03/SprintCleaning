using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingMenu : MonoBehaviour
{
    private Level_Tracker _levelTracker => Level_Tracker.Instance;


    public void MainMenuButton()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void RunAgainButton()
    {
        SceneManager.LoadScene("Level " + _levelTracker._currentLevel);
    }

    public void NextLevelButton()
    {
        int currentLevel = _levelTracker._currentLevel;

        switch (currentLevel)
        {

            case 1:
                if(_levelTracker.LevelsUnlocked() > 1)
                    SceneManager.LoadScene("Level 2");
                break;

            case 2:
                if (_levelTracker.LevelsUnlocked() > 2)
                    SceneManager.LoadScene("Level 3");
                break;

            case 3:
                SceneManager.LoadScene("Level 1");
                break;
        }
    }
}
