using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

public class ScoreTextUpdated : CustomText
{
    [SerializeField]
    private TextMeshProUGUI _scoreTextUpdated;
    [SerializeField]
    public DataManager _dataManager => DataManager.Instance;
    private void Start()
    {
        if (_dataManager == null)
            return;
        int score = ScoreManager._score;
        _scoreTextUpdated.text = "Total Score: " + score;
    }
}
