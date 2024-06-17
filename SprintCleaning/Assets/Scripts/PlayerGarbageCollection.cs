using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class PlayerGarbageCollection : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _garbageText;

    [SerializeField]
    private ScoreManager _scoreManager;

    [SerializeField]
    private TextMeshProUGUI _scoreText;

    [SerializeField]
    private float _streakDecreaseInterval = 1f;

    
    private void Start()
    {
        //_scoreManager.Awake();
        InvokeRepeating(nameof(DecreaseStreak), 1f, _streakDecreaseInterval);
    }
    public void TextEdit()
    {
        //if (_garbageText != null)
        //{
        //    string text = "";
        //    for (int i = 0; i < (int)(GarbageType.Count); i++)
        //        text += (GarbageType)i + ": " + _playerData._counts[(GarbageType)i] + "\n";

        //    _garbageText.text = text;
        //}

        if (_scoreText != null)
        {
            string scoreText = "Score: " + _scoreManager._score;
            string streakValueText = "StreakValue: " + _scoreManager._streakValue;
            string streakText = "Streak: " + _scoreManager._streakMultiplier;

            _scoreText.text = scoreText + "\n" + streakValueText + "\n" + streakText;
        }
    }

    private void DecreaseStreak()
    {
        _scoreManager.DecreaseStreak();
        TextEdit();
    }

}
