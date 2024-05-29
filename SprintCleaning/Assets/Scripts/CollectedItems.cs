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
    public Dictionary<ToolType, int> _counts;
    public void Awake()
    {
        _counts = new Dictionary<ToolType, int>
        {
            { ToolType.Broom, _totalBroomGarbageCount },
            { ToolType.Mop, _totalMopGarbageCount },
            { ToolType.GarbageBag, _totalBagGarbageCount }
        };

    }
    public void GarbageCollected(ToolType garbage)
    {
        _counts[garbage]++;
        _totalBagGarbageCount = _counts[ToolType.GarbageBag];
        _totalBroomGarbageCount = _counts[ToolType.Broom];
        _totalMopGarbageCount = _counts[ToolType.Mop];
    }
    
}
