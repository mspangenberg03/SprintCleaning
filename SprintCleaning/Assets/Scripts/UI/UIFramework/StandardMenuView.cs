using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StandardMenuView : CustomUIComponent
{
    public StandardMenuSO _menuSO;

    public GameObject _header;
    public GameObject _footer;
    public GameObject _body;

    private Image _headerImage;
    private Image _footerImage;
    private Image _bodyImage;

    public override void Setup()
    {
        _headerImage = _header.GetComponent<Image>();
        _footerImage = _footer.GetComponent<Image>();
        _bodyImage = _body.GetComponent<Image>();
    }

    public override void Configure()
    {
        _headerImage.color = _menuSO._theme.GetBackgroundColor(Style.HeaderFirst);
        _bodyImage.color = _menuSO._theme.GetBackgroundColor(Style.BodyFirst);
        _footerImage.color = _menuSO._theme.GetBackgroundColor(Style.FooterFIrst);
    }

}
