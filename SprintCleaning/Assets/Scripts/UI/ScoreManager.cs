using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    //[SerializeField] private bool _resetInEditorOnAwake = true;
    //The garbage that the player has in total
    public static Dictionary<GarbageType, int> _counts;

    public static int _score = 0;

    public int _streakValue = 30;

    [SerializeField, Tooltip("Value by which streakValue is decreased through the game")]
    private int _regularStreakDecrease = 5;

    public int _streakMultiplier = 1;

    public int _powerUpMultiplier = 1;

    private Game_Over _gameOver;

    [field: SerializeField] public int MaxStreakValue { get; private set; }

    [SerializeField]
    private int[] _streakThresholds;

    public int[] StreakThresholds => _streakThresholds;

    [SerializeField]
    private StreakBar _streakBar;
    private GameObject _levelTracker;
    private Level_Tracker _levelCode;

    private static ScoreManager _instance;
    public static ScoreManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ScoreManager>();
            }
            return _instance;
        }
    }

    public void Awake()
    {

        _instance = this;
        _levelTracker = GameObject.Find("levelTracker");
        if(_levelTracker != null)
            _levelCode = _levelTracker.GetComponent<Level_Tracker>();



        _score = 0;
        //if (_counts == null)
        {
            _counts = new Dictionary<GarbageType, int>();
            for (int i = 0; i < (int)(GarbageType.Count); i++)
                _counts.Add((GarbageType)i, 0);
        }

        _streakBar._current = _streakValue;

        GameObject player = GameObject.Find("Player");
        _gameOver = player.GetComponent<Game_Over>();


        //#if UNITY_EDITOR
        //        if (_resetInEditorOnAwake)
        //        {
        //            for (int i = 0; i < (int)(GarbageType.Count); i++)
        //            {
        //                _counts[(GarbageType)i] = 0;
        //            }
        //        }
        //#endif
    }

    public void GarbageCollected(GarbageType garbage)
    {
        _counts[garbage]++;
    }

    public void AddScoreOnGarbageCollection(int scoreToAdd, int streakValueToAdd)
    {
        CheckStreakMultiplier();
        int add = scoreToAdd * _streakMultiplier * _powerUpMultiplier;
        ScoreGainText.Instance.OnScoreGained(add);
        _score += add;
        _streakValue += streakValueToAdd;
        _streakValue = System.Math.Min(MaxStreakValue, _streakValue);
        _streakBar._current = _streakValue;
        CheckForNextLevel();
    }

    public void DecreaseStreak()
    {
        _streakValue -= _regularStreakDecrease;
        _streakValue = System.Math.Max(0, _streakValue);
        
        if(_streakValue == 0){
            if (!Game_Over.Instance.GameIsOver){
                GameObject player = GameObject.Find("Player");
                Animator animator = player.GetComponentInChildren<Animator>();
                animator.SetTrigger("Hit");
                _gameOver.GameOver();
            }
        }
        CheckStreakMultiplier();
        _streakBar._current = _streakValue;
    }

    public void CheckStreakMultiplier()
    {
        int priorStreakMultiplier = _streakMultiplier;

        for (int i = 0; i < _streakThresholds.Length; i++)
        {
            if (_streakValue <= _streakThresholds[i])
            {
                _streakMultiplier = i + 1;
                break;
            }
        }
        if (_streakValue > _streakThresholds[^1]){
            _streakMultiplier = _streakThresholds.Length + 1;
        }


        if (_streakMultiplier == priorStreakMultiplier)
            return;

        float duration = .5f;
        StartCoroutine(FadeMixerGroup.StartFade(GameplayMusic.Instance.GameAudioMixer, "Streak1Volume", duration, 1));
        StartCoroutine(FadeMixerGroup.StartFade(GameplayMusic.Instance.GameAudioMixer, "Streak2Volume", duration, _streakMultiplier >= 2 ? 1 : 0));
        StartCoroutine(FadeMixerGroup.StartFade(GameplayMusic.Instance.GameAudioMixer, "Streak3Volume", duration, _streakMultiplier >= 3 ? 1 : 0));
        StartCoroutine(FadeMixerGroup.StartFade(GameplayMusic.Instance.GameAudioMixer, "Streak4Volume", duration, _streakMultiplier >= 4 ? 1 : 0));
    }

    public void OnGameEnds()
    {
        StopAllCoroutines();
    }

    private void CheckForNextLevel()
    {
#if UNITY_EDITOR
        if (_levelCode != null && _levelCode._isLevelCompletedOnScore)
        {
            if (_score >= _levelCode._nextLevelThreshold)
            {
                _levelCode.UnlockLevel();
                _gameOver.LevelComplete();
            }
        }
#endif
        if (_streakValue >= MaxStreakValue)
        {
            if (_levelCode != null)
            {
                _levelCode.UnlockLevel();
                Debug.Log("Unlocked New Level!");
            }
            _gameOver.LevelComplete();
        }
    }
}