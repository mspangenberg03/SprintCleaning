using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//[ExecuteInEditMode()]
public class StreakBar : MonoBehaviour
{
    public int _current;
    public Image _mask;
    [SerializeField]
    private RectTransform[] _dividers;

    void Start()
    {
        CalculateThresholdPosition();
    }

    void Update()
    {
        GetCurrentStreakFill();
    }

    public void GetCurrentStreakFill()
    {
        float fillAmount = (float)_current / (float)ScoreManager.Instance.MaxStreakValue;
        _mask.fillAmount = fillAmount;
    }

    private void CalculateThresholdPosition()
    {
        float barLenght = GetComponent<RectTransform>().rect.width;
        int index = 0;
        int[] thresholds = ScoreManager.Instance.StreakThresholds;
        int maxStreakValue = ScoreManager.Instance.MaxStreakValue;
        foreach (int threshold in thresholds)
        {
            float dividerPosition = (float)threshold / (float)maxStreakValue * barLenght;
            _dividers[index].anchoredPosition = new Vector2(dividerPosition, 0);
            index++;
        }
    }
}
