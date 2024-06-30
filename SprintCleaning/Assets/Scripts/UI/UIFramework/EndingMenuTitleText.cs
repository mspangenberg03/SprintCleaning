using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndingMenuTitleText : CustomText
{
    private Level_Tracker _levelTracker => Level_Tracker.Instance;
    void Start()
    {
        if (_levelTracker._gameOver.GameIsOver)
            _text.text = "Game Over";
        else
            _text.text = "Level " + _levelTracker._currentLevel + " Complete";
    }

}
