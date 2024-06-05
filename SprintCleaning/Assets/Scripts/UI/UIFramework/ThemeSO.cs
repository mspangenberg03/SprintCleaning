using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName ="CustomUI/ThemeSO", fileName ="ThemeSO")]
public class ThemeSO : ScriptableObject
{
    [Header("HeaderFirst")]
    public Color _headerFirst_bg;
    public Color _headerFirst_txt;

    [Header("BodyFirst")]
    public Color _bodyFirst_bg;
    public Color _bodyFirst_txt;

    [Header("BodySecond")]
    public Color _bodySecond_bg;
    public Color _bodySecond_txt;

    [Header("DescriptionBox")]
    public Color _descBox_bg;
    public Color _descBox_txt;

    [Header("ButtonGeneric")]
    public Color _buttonGeneric_bg;
    public Color _buttonGeneric_txt;

    [Header("FooterFirst")]
    public Color _footerFirst_bg;
    public Color _footerFirst_txt;

    [Header("Disable")]
    public Color _disable;

    public Color GetBackgroundColor(Style style)
    {
        switch (style)
        {
            case Style.HeaderFirst:
                return _headerFirst_bg;

            case Style.BodyFirst:
                return _bodyFirst_bg;

            case Style.BodySecond:
                return _bodySecond_bg;

            case Style.DescriptionBox:
                return _descBox_bg;

            case Style.ButtonGeneric:
                return _buttonGeneric_bg;

            case Style.FooterFIrst:
                return _footerFirst_bg;

            case Style.Disable:
                return _disable;

            default:
                return Color.black;
        }
    }

    public Color GetTextColor(Style style)
    {
        switch (style)
        {
            case Style.HeaderFirst:
                return _headerFirst_txt;

            case Style.BodyFirst:
                return _bodyFirst_txt;

            case Style.BodySecond:
                return _bodySecond_txt;

            case Style.DescriptionBox:
                return _descBox_txt;

            case Style.ButtonGeneric:
                return _buttonGeneric_txt;

            case Style.FooterFIrst:
                return _footerFirst_txt;

            case Style.Disable:
                return _disable;

            default:
                return Color.black;
        }
    }

}
