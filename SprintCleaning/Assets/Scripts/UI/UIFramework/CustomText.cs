using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomText : CustomUIComponent
{
    public TextSO _textData;
    protected TextMeshProUGUI _text;

    public override void Setup()
    {
        _text = GetComponentInChildren<TextMeshProUGUI>();
    }

    public override void Configure()
    {
        _text.font = _textData._font;
        _text.fontSize = _textData._size;
        _text.color = _textData._theme.GetTextColor(_textData._style);
    }
}
