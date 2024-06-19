using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game_Over : MonoBehaviour
{
    private float _gameOverDelay = 2f;
    public static bool GameIsOver {private set; get; }

    private void Start()
    {
        GameIsOver = false;
    }

    public void GameOver()
    {
        StartCoroutine(DelayGameOver());
    }

    IEnumerator DelayGameOver()
    {
        GameIsOver = true;
        yield return new WaitForSeconds(_gameOverDelay);
        SceneManager.LoadScene("MainMenu");
    }
}
