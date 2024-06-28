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
    private ParticleSystem _particle;

    
    private void Start()
    {
        //_scoreManager.Awake();
        InvokeRepeating(nameof(DecreaseStreak), 1f, _streakDecreaseInterval);
        _particle = GetComponent<ParticleSystem>();
    }
    public void TextEdit()
    {
        if (_garbageText != null)
        {
            string text = "";
            for (int i = 0; i < (int)(GarbageType.Count); i++)
                text += (GarbageType)i + ": " + _scoreManager._counts[(GarbageType)i] + "\n";

            _garbageText.text = text;
        }

        if (_scoreText != null)
        {
            string scoreText = "Score: " + _scoreManager._score;

            _scoreText.text = scoreText;
        }
    }

    private void DecreaseStreak()
    {
        _scoreManager.DecreaseStreak();
        TextEdit();
    }
}
