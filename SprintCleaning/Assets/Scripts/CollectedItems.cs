using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu]
public class CollectedItems : ScriptableObject
{

    //The garbage that the player has in total
    private int _totalBagGarbageCount = 0;
    private int _totalBroomGarbageCount = 0;
    private int _totalMopGarbageCount = 0;

    public void TextEdit()
    {
        GameObject player = GameObject.Find("Player");
        TextMeshProUGUI text = player.GetComponent<PlayerGarbageCollection>()._garbageText;
        if(text != null)
        {
            text.text = "Broom Garbage: " + _totalBroomGarbageCount +"\n" +
                        "Mop Garbage: " + _totalMopGarbageCount +"\n" +
                        "Bag Garbage: " + _totalBagGarbageCount;
        }
       
        
    }
    public void GarbageCollected(ToolType garbage)
    {
        
        if(garbage == ToolType.Broom)
        {
            _totalBroomGarbageCount++;
        }
        if(garbage == ToolType.GarbageBag)
        {

            _totalBagGarbageCount++;
        }
        if(garbage == ToolType.Mop)
        {
            _totalMopGarbageCount++;
        }
    }
}
