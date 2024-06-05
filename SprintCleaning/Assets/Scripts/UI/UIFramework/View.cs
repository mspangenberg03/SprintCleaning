using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class View : CustomUIComponent
{
    public ViewSO _viewData;

    public GameObject _containerLeft;
    public GameObject _containerRight;

    private Image _imageLeft;
    private Image _imageRight;

    private HorizontalLayoutGroup _horizontalLayout;

    public override void Setup()
    {
        _horizontalLayout = GetComponent<HorizontalLayoutGroup>();
        _imageLeft = _containerLeft.GetComponent<Image>();
        _imageRight = _containerRight.GetComponent<Image>();
    }

    public override void Configure()
    {
        _horizontalLayout.padding = _viewData._padding;
        _horizontalLayout.spacing = _viewData._spacing;

        _imageLeft.color = _viewData._theme.GetBackgroundColor(Style.HeaderFirst);
        _imageRight.color = _viewData._theme.GetBackgroundColor(Style.BodyFirst);
    }
}
