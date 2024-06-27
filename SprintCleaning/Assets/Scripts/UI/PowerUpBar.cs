using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpBar : MonoBehaviour
{
    private PlayerPowerUpManager PlayerPowerUpManager => GameObject.FindObjectOfType<PlayerPowerUpManager>();
    [SerializeField] private Image[] _PowerUpIcons;
    [SerializeField] private Slider[] _durabilities;

    public void UpdateDisplayedInfo(List<PowerUpBase> heldPowerUps)
    {
        /*
        for (int i = 0; i < heldPowerUps.Count; i++)
        {
            _PowerUpIcons[i].enabled = true;
            _PowerUpIcons[i].overrideSprite = heldPowerUps[i]._PowerUpUI.Sprite;

            _durabilities[i].gameObject.SetActive(true);
            _durabilities[i].value = Mathf.InverseLerp(heldPowerUps[i]._durablity, 0, heldPowerUps[i]._PowerUpUses);

            float alpha = heldPowerUps[i]._PowerUpUses == heldPowerUps[i]._durablity - 1 ? .66f : 1f;
            _PowerUpIcons[i].color = new Color(1f, 1f, 1f, alpha);
        }
        for (int i = heldPowerUps.Count; i < _PowerUpIcons.Length; i++)
        {
            _PowerUpIcons[i].enabled = false;
            _durabilities[i].gameObject.SetActive(false);
        }
        */
    }
}
