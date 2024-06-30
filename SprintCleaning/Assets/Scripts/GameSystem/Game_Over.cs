using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game_Over : MonoBehaviour
{
    [SerializeField] private Animator _playerAnimator;

    private float _gameOverDelay = 2f;
    private float _gameOverDelayStartTime;
    public bool GameIsOver {private set; get; }
    public GameObject _generalUI;
    public float FractionOfGameOverDelayElapsed => !GameIsOver ? 0 : Mathf.InverseLerp(_gameOverDelayStartTime, _gameOverDelayStartTime + _gameOverDelay, Time.time);

    private static Game_Over _instance;
    public static Game_Over Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<Game_Over>();
            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
    }

    public void GameOver()
    {
        if (DevHelper.Instance.ImmortalPlayer)
            return;
        if (GameIsOver)
            return;
        StartCoroutine(DelayGameOver());
    }

    IEnumerator DelayGameOver()
    {
        GameIsOver = true;

        ScoreManager.Instance.OnGameEnds();
        GameplayMusic.Instance.OnGameEnds();

        _gameOverDelayStartTime = Time.time;

        if (!_playerAnimator.GetBool("Idle"))
        {
            _playerAnimator.SetBool("Idle", true);
            _playerAnimator.SetFloat("Speed", 0f);
        }


        yield return new WaitForSeconds(_gameOverDelay);
        _generalUI.SetActive(false);
        SceneManager.LoadScene("EndingMenu");
    }
}
