using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class CustomButton : CustomUIComponent
{
    public ThemeSO _theme;
    public Style _style;
    public UnityEvent _onClick;

    private Button _button;
    private TextMeshProUGUI _buttonText;
    public override void Setup()
    {
        _button = GetComponentInChildren<Button>();
        _buttonText = GetComponentInChildren<TextMeshProUGUI>();
    }
    public override void Configure()
    {
        ColorBlock colorBlock = _button.colors;
        colorBlock.normalColor = _theme._buttonGeneric_bg;
        _button.colors = colorBlock;

        _buttonText.color = _theme._buttonGeneric_txt;
    }

    public void OnClick()
    {
        _onClick.Invoke();
    }
}
