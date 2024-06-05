using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolBar : MonoBehaviour
{
    private PlayerToolManager PlayerToolManager => GameObject.FindObjectOfType<PlayerToolManager>();
    [SerializeField] private Image[] _toolIcons;
    [SerializeField] private Slider[] _durabilities;

    public void UpdateDisplayedInfo(List<ToolBase> heldTools)
    {
        for (int i = 0; i < heldTools.Count; i++)
        {
            _toolIcons[i].enabled = true;
            _toolIcons[i].overrideSprite = heldTools[i]._toolUI.Sprite;

            _durabilities[i].gameObject.SetActive(true);
            _durabilities[i].value = Mathf.InverseLerp(heldTools[i]._durablity, 0, heldTools[i]._toolUses);

            float alpha = heldTools[i]._toolUses == heldTools[i]._durablity - 1 ? .66f : 1f;
            _toolIcons[i].color = new Color(1f, 1f, 1f, alpha);
        }
        for (int i = heldTools.Count; i < _toolIcons.Length; i++)
        {
            _toolIcons[i].enabled = false;
            _durabilities[i].gameObject.SetActive(false);
        }
    }
}
