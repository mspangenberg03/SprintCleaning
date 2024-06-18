using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[ExecuteInEditMode()]
public class StreakBar : MonoBehaviour
{

    public int _maximum;
    public int _minimum;
    public int _current;
    public Image _mask;
    [SerializeField]
    private RectTransform[] _dividers;
    private int[] _streakThresholds => ScoreManager.Instance.StreakThresholds;

    // Start is called before the first frame update
    void Start()
    {
        //_dividers = new RectTransform[_streakThresholds.Length];
        //for (int i = 0; i < _streakThresholds.Length; i++)
        //{
        //    _dividers[i] = GetComponentsInChildren<RectTransform>()[i + 3];
        //}
        CalculateThresholdPosition();
    }

    // Update is called once per frame
    void Update()
    {
        GetCurrentStreakFill();
    }

    public void GetCurrentStreakFill()
    {
        float currentOffset = _current - _minimum;
        float maximumOffset = _maximum - _minimum;
        float fillAmount = (float)currentOffset / (float)maximumOffset;
        _mask.fillAmount = fillAmount;
    }

    private void CalculateThresholdPosition()
    {
        float barLenght = GetComponent<RectTransform>().rect.width;
        int index = 0;
        Debug.Log("barLength " + barLenght);
        foreach (int threshold in _streakThresholds)
        {
            Debug.Log("threshold " + threshold);
             float dividerPosition = (float)threshold / ((float)_maximum - (float)_minimum) * barLenght;
            _dividers[index].anchoredPosition = new Vector2(dividerPosition, 0);
            Debug.Log("anchoredPos" + _dividers[index].anchoredPosition.x);
            Debug.Log("dividerPos " + dividerPosition);
            index++;
        }
    }
}
