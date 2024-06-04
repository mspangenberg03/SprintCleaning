using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CustomUI/ViewSO", fileName = "ViewSO")]
public class ViewSO : ScriptableObject
{
    public ThemeSO _theme;
    public RectOffset _padding;
    public float _spacing;
}
