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
    public Dictionary<ToolType, int> _counts = new Dictionary<ToolType, int>
    {
         {ToolType.Broom,0},
         {ToolType.Mop, 0},
         {ToolType.GarbageBag, 0}
    };
    public void GarbageCollected(ToolType garbage)
    {
        _counts[garbage]++;
    }
    //Saves the total garbage collected
    public void GameOver()
    {
        _totalBagGarbageCount += _counts[ToolType.GarbageBag];
        _totalBroomGarbageCount += _counts[ToolType.Broom];
        _totalMopGarbageCount += _counts[ToolType.Mop];
    }
}
