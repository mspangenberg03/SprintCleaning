using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StreakBar : MonoBehaviour, IOnStreakChanges
{
    public Image _mask;
    [SerializeField] private Image _fillImage;
    [SerializeField] private Color _mainColor;
    [SerializeField] private BarColorLerp _lowBarColorTransition;
    [SerializeField] private BarColorLerp _fullBarColorTransition;
    [SerializeField] private RectTransform[] _dividers;

    [System.Serializable]
    private class BarColorLerp
    {
        [Header("Start & end are fraction full.")]
        [SerializeField] private float _start;
        [SerializeField] private float _end;
        [SerializeField] private float _shapeTPower = 1;
        [SerializeField] private bool _invertBeforePower;
        [SerializeField] private Color _color = Color.white;

        public Color Lerp(Color from, float barFractionFull)
        {
            float t = Mathf.InverseLerp(_start, _end, barFractionFull);
            if (_invertBeforePower)
                t = 1 - Mathf.Pow(1 - t, _shapeTPower);
            else
                t = Mathf.Pow(t, _shapeTPower);
            return Color.Lerp(from, _color, t);
        }


    }

    private void Awake()
    {
        ScoreManager.Instance.AddInformStreak(this);
    }

    void Start()
    {
        CalculateThresholdPosition();
    }

    public void OnStreakChanges(int newStreak)
    {
        UpdateFill(newStreak);
    }

    private void UpdateFill(int streak)
    {
        float fillAmount = (float)streak / ScoreManager.Instance.MaxStreakValue;
        
        _mask.fillAmount = fillAmount;

        Color c = _mainColor;
        c = _lowBarColorTransition.Lerp(c, fillAmount);
        c = _fullBarColorTransition.Lerp(c, fillAmount);
        _fillImage.color = c;
    }

    private void CalculateThresholdPosition()
    {
        float barLength = GetComponent<RectTransform>().rect.width;
        int index = 0;
        int[] thresholds = ScoreManager.Instance.StreakThresholds;
        int maxStreakValue = ScoreManager.Instance.MaxStreakValue;
        foreach (int threshold in thresholds)
        {
            float dividerPosition = (float)threshold / maxStreakValue * barLength;
            _dividers[index].anchoredPosition = new Vector2(dividerPosition, 0);
            index++;
        }
    }

    
}
