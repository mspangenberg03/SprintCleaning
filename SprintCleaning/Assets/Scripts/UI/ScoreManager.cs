using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    //The garbage that the player has in total
    public static Dictionary<GarbageType, int> _countsForEndScreen;

    public static int _score = 0;

    public int _streakValue = 30;
    private float _streakDecreaseAccumulator;
    private float _streakIncreaseAccumulator;

    [SerializeField] private float _streakDecreasePerSec = 5;
    [SerializeField] private float _streakDecreaseWhenPassGarbage = 3;
    [SerializeField] private float _minIntervalToPunishPassGarbage = 3;
    [SerializeField] private float _streakIncreaseScale = 1f;
    [SerializeField] private StreakBar _streakBar;

    public int _streakMultiplier = 1;

    public int _powerUpMultiplier = 1;

    private float _lastPassGarbageTime = float.NegativeInfinity;

    [field: SerializeField] public int MaxStreakValue { get; private set; }

    [SerializeField] private int[] _streakThresholds;

    public int[] StreakThresholds => _streakThresholds;

    public Dictionary<GarbageType, int> Counts { get; private set; }

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
                _instance.Counts = new Dictionary<GarbageType, int>();
                for (int i = 0; i < (int)(GarbageType.Count); i++)
                    _instance.Counts.Add((GarbageType)i, 0);
            }
            return _instance;
        }
    }

    private List<IOnScoreChanges> _informScore = new();
    private List<IOnStreakChanges> _informStreak = new();

    public void AddInformScore(IOnScoreChanges toInform)
    {
        _informScore.Add(toInform);
        toInform.OnScoreChanges(_score, 0);
    }

    public void AddInformStreak(IOnStreakChanges toInform)
    {
        _informStreak.Add(toInform);
        toInform.OnStreakChanges(_streakValue);
    }

    private void Awake()
    {
        _instance = this;
        _levelTracker = GameObject.Find("levelTracker");
        if (_levelTracker != null)
            _levelCode = _levelTracker.GetComponent<Level_Tracker>();

        _score = 0;
    }

    public void OnPassGarbage()
    {
        if (Time.time < _lastPassGarbageTime + _minIntervalToPunishPassGarbage)
            return;
        _lastPassGarbageTime = Time.time;
        _streakDecreaseAccumulator += _streakDecreaseWhenPassGarbage;
        _streakBar.OnMissGarbage();
    }

    private void Update()
    {
        _streakDecreaseAccumulator += Time.deltaTime * _streakDecreasePerSec;
        int decrease = (int)_streakDecreaseAccumulator;
        _streakDecreaseAccumulator -= decrease;

        DecreaseStreak(decrease);
    }

    private void OnDestroy()
    {
        _countsForEndScreen = Counts; // probably should copy the dictionary or something so not still referencing an object from a destroyed gameObject
    }

    public void GarbageCollected(GarbageType garbage)
    {
        Counts[garbage]++;
    }

    public void AddScoreOnGarbageCollection(int scoreToAdd, int streakValueToAdd)
    {
        CheckStreakMultiplier();
        int add = scoreToAdd * _streakMultiplier * _powerUpMultiplier;
        if (add != 0) 
        {
            _score += add;
            OnScoreChanges(add);
        }

        if (streakValueToAdd != 0)
        {
            _streakIncreaseAccumulator += streakValueToAdd * _streakIncreaseScale;
            streakValueToAdd = (int)_streakIncreaseAccumulator;
            _streakIncreaseAccumulator -= streakValueToAdd;

            _streakValue += streakValueToAdd;
            _streakValue = System.Math.Min(MaxStreakValue, _streakValue);
            OnStreakChanges();
        }


        CheckForNextLevel();
    }

    private void OnScoreChanges(int add)
    {
        for (int i = 0; i < _informScore.Count; i++)
            _informScore[i].OnScoreChanges(_score, add);
    }

    private void OnStreakChanges()
    {
        for (int i = 0; i < _informStreak.Count; i++)
            _informStreak[i].OnStreakChanges(_streakValue);
    }

    private void DecreaseStreak(int decreaseAmount)
    {
        if (decreaseAmount == 0)
            return;

        if (!Game_Over.Instance.GameIsOver && !Game_Over.Instance.LevelIsComplete)
        {
            _streakValue -= decreaseAmount;
            _streakValue = System.Math.Max(0, _streakValue);
            OnStreakChanges();
        }
        
        if (_streakValue == 0)
        {
            if (!Game_Over.Instance.GameIsOver)
            {
                GameObject player = GameObject.Find("Player");
                Animator animator = player.GetComponentInChildren<Animator>();
                animator.SetTrigger("Hit");
                Game_Over.Instance.GameOver();
            }
        }
        CheckStreakMultiplier();
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

        const float duration = .5f;
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
                Game_Over.Instance.LevelComplete();
            }
        }
#endif
        if (_streakValue >= MaxStreakValue)
        {
            if (_levelCode != null)
                _levelCode.UnlockLevel();
            Game_Over.Instance.LevelComplete();
        }
    }
}