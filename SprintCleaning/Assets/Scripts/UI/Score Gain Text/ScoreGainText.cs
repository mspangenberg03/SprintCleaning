using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class ScoreGainText : MonoBehaviour, IOnScoreChanges
{
    [SerializeField] private Animator _anim;
    [SerializeField] private TMPro.TextMeshProUGUI _text;

    private Queue<int> _scoresGained = new Queue<int>();
    private bool _awaitingAnimationEnd;
    private int _showScoreGainedID;

    private static ScoreGainText _instance;
    public static ScoreGainText Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<ScoreGainText>();
            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
        _showScoreGainedID = Animator.StringToHash("ShowScoreGained");

        ScoreManager.Instance.AddInformScore(this);
    }

    public void OnScoreChanges(int newScore, int scoreChange)
    {
        if (!_awaitingAnimationEnd)
            PlayAnimation(scoreChange);
        else
            _scoresGained.Enqueue(scoreChange);
    }

    public void OnScoreAnimationEnds()
    {
        if (_scoresGained.Count == 0)
            _awaitingAnimationEnd = false;
        else
            PlayAnimation(_scoresGained.Dequeue());
    }

    private void PlayAnimation(int scoreGained)
    {
        _awaitingAnimationEnd = true;
        _text.text = "+" + scoreGained;
        _anim.SetTrigger(_showScoreGainedID);
    }

   
}
