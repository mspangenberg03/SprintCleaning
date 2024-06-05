using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[CreateAssetMenu(menuName = "CustomUI/TextSO", fileName = "TextSO")]
public class TextSO : ScriptableObject
{
    public ThemeSO _theme;
    public Style _style;
    public TMP_FontAsset _font;
    public float _size;
}
