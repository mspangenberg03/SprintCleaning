using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolBar : MonoBehaviour
{
    private PlayerToolManager PlayerToolManager => GameObject.FindObjectOfType<PlayerToolManager>();
    private List<ToolBase> ActiveTools => PlayerToolManager._toolList;
    private Image[] _toolSprites => gameObject.GetComponentsInChildren<Image>();
    
    public void DrawSpritesOnToolAdd()
    {
        if (ActiveTools.Count == 1)
        {
            _toolSprites[2].overrideSprite = ActiveTools[0]._toolUI.Sprite;
        }
        else
        {
            for (int i = 0; i < ActiveTools.Count; i++)
            {
                if (ActiveTools[i] != null)

                    _toolSprites[i + 1].overrideSprite = ActiveTools[i]._toolUI.Sprite;
            }
        }

    }
}
