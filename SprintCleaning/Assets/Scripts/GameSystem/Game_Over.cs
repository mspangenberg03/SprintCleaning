using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game_Over : MonoBehaviour
{
    [SerializeField] private Animator _playerAnimator;


    private float _gameOverDelay = 2f;
    private float _gameOverDelayStartTime;

    private float _levelCompleteDelay = 1.5f;
    public bool GameIsOver {private set; get; }
    public bool LevelIsComplete { private set; get; }
    public GameObject _generalUI;
    public float FractionOfGameOverDelayElapsed => !GameIsOver && !LevelIsComplete ? 0 : Mathf.InverseLerp(_gameOverDelayStartTime, _gameOverDelayStartTime + _gameOverDelay, Time.time);
    public static bool RunEndedByLosing { get; private set; }

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
        RunEndedByLosing = false;
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
        RunEndedByLosing = true;
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
        //SceneManager.LoadScene("EndingMenu");
    }

    public void LevelComplete()
    {
        if (LevelIsComplete)
            return;
        StartCoroutine(LevelCompleteDelay());
    }

    public IEnumerator LevelCompleteDelay()
    {
        LevelIsComplete = true;

        ScoreManager.Instance.OnGameEnds();
        GameplayMusic.Instance.OnGameEnds();

        _gameOverDelayStartTime = Time.time;

        _playerAnimator.SetTrigger("Cheer");
        _playerAnimator.SetFloat("Speed", 0f);

        yield return new WaitForSeconds(_levelCompleteDelay);
        _generalUI.SetActive(false);
        SceneManager.LoadScene("EndingMenu");
    }
}
