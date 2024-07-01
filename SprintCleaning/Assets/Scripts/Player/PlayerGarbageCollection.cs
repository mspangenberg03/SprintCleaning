using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerGarbageCollection : MonoBehaviour, IOnScoreChanges
{
    [SerializeField]
    private TextMeshProUGUI _garbageText;

    [SerializeField]
    private TextMeshProUGUI _scoreText;

    private void Awake()
    {
        ScoreManager.Instance.AddInformScore(this); // Need to use Instance to construct ScoreManager._counts
        TextEdit(0);
    }

    public void OnScoreChanges(int newScore, int scoreChange)
    {
        TextEdit(newScore);
    }

    private void TextEdit(int score)
    {
        if (_garbageText != null)
        {
            string text = "";
            for (int i = 0; i < (int)(GarbageType.Count); i++)
                text += (GarbageType)i + ": " + ScoreManager.Instance.Counts[(GarbageType)i] + "\n";

            _garbageText.text = text;
        }

        if (_scoreText != null)
        {
            string scoreText = "Score: " + score;

            _scoreText.text = scoreText;
        }
    }

   
}
