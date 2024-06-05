using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Text : CustomUIComponent
{
    public TextSO _textData;
    private TextMeshProUGUI _textMeshProUGUI;

    public override void Setup()
    {
        _textMeshProUGUI = GetComponentInChildren<TextMeshProUGUI>();
    }

    public override void Configure()
    {
        _textMeshProUGUI.font = _textData._font;
        _textMeshProUGUI.fontSize = _textData._size;
        _textMeshProUGUI.color = _textData._theme.GetTextColor(_textData._style);
    }
}
