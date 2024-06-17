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
    // Start is called before the first frame update
    void Start()
    {
        
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
}
