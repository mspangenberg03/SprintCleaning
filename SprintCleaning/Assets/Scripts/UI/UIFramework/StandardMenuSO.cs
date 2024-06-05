using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="CustomUI/MenuStandardSO", fileName ="MenuStandardSO")]
public class StandardMenuSO : ScriptableObject
{
    public ThemeSO _theme;
    public RectOffset _padding;
    public float _spacing;
}
