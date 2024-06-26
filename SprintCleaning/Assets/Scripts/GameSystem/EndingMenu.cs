using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingMenu : MonoBehaviour
{



    public void MainMenuButton()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void RunAgainButton()
    {
        SceneManager.LoadScene("Main");
    }

}
