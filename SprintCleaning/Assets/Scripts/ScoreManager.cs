using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [SerializeField]
    private int[] _streakThresholds;

    [SerializeField]
    private StreakBar _streakBar;

    //public void Awake()
    //{
    //    if (_counts == null)
    //    {
    //        _counts = new Dictionary<GarbageType, int>();
    //        for (int i = 0; i < (int)(GarbageType.Count); i++)
    //            _counts.Add((GarbageType)i, 0);
    //    }

    //    #if UNITY_EDITOR
    //    if (_resetInEditorOnAwake)
    //    {
    //        for (int i = 0; i < (int)(GarbageType.Count); i++)
    //        {
    //            _counts[(GarbageType)i] = 0;
    //        }
    //        _score = 0;
    //        _streakValue = 30;
    //    }
    //#endif
    //}

    public void GarbageCollected(GarbageType garbage)
    {
        _counts[garbage]++;
    }

    public void AddScoreOnGarbageCollection(int scoreToAdd, int streakValueToAdd)
    {
        CheckStreakMultiplier();
        _score += (scoreToAdd * _streakMultiplier);
        _streakValue += streakValueToAdd;
        _streakBar._current += streakValueToAdd;
    }

    public void DecreaseStreak()
    {
        _streakValue -= _regularStreakDecrease;
        CheckStreakMultiplier();
    }

    private void CheckStreakMultiplier()
    {
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
    }
}