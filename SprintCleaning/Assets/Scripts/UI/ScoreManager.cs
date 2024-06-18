using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class ScoreManager : MonoBehaviour
{
    //[SerializeField] private bool _resetInEditorOnAwake = true;
    //The garbage that the player has in total
    public Dictionary<GarbageType, int> _counts;

    [Tooltip("Total score")]
    public int _score = 0;

    public int _streakValue = 30;

    [SerializeField, Tooltip("Value by which streakValue is decreased through the game")]
    private int _regularStreakDecrease = 5;

    public int _streakMultiplier = 1;

    [field: SerializeField] public int MaxStreakValue { get; private set; }

    [SerializeField]
    private int[] _streakThresholds;

    [SerializeField] private AudioMixer gameAudioMixer;

    public int[] StreakThresholds => _streakThresholds;

    [SerializeField]
    private StreakBar _streakBar;

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

        if (_counts == null)
        {
            _counts = new Dictionary<GarbageType, int>();
            for (int i = 0; i < (int)(GarbageType.Count); i++)
                _counts.Add((GarbageType)i, 0);
        }

        _streakBar._current = _streakValue;

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
        _score += (scoreToAdd * _streakMultiplier);
        _streakValue += streakValueToAdd;
        _streakBar._current = _streakValue;
    }

    public void DecreaseStreak()
    {
        _streakValue -= _regularStreakDecrease;
        CheckStreakMultiplier();
        _streakBar._current = _streakValue;
    }

    private void CheckStreakMultiplier()
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
        if (_streakValue > _streakThresholds[^1])
            _streakMultiplier = _streakThresholds.Length + 1;

        if (_streakMultiplier == priorStreakMultiplier)
            return;

        float duration = .5f;
        StartCoroutine(FadeMixerGroup.StartFade(gameAudioMixer, "Streak1Volume", duration, 1));
        StartCoroutine(FadeMixerGroup.StartFade(gameAudioMixer, "Streak2Volume", duration, _streakMultiplier >= 2 ? 1 : 0));
        StartCoroutine(FadeMixerGroup.StartFade(gameAudioMixer, "Streak3Volume", duration, _streakMultiplier >= 3 ? 1 : 0));
        StartCoroutine(FadeMixerGroup.StartFade(gameAudioMixer, "Streak4Volume", duration, _streakMultiplier >= 4 ? 1 : 0));
    }
}