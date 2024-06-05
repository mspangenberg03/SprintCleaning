using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu]
public class CollectedItems : ScriptableObject
{
    [SerializeField] private bool _resetInEditorOnAwake = true;

    //The garbage that the player has in total
    private int _totalBagGarbageCount = 0;
    private int _totalBroomGarbageCount = 0;
    private int _totalMopGarbageCount = 0;
    private int _totalPurpleGarbageCount = 0;
    private int _totalYellowGarbageCount = 0;
    public Dictionary<ToolType, int> _counts;
    public void Awake()
    {
#if UNITY_EDITOR
        if (_resetInEditorOnAwake)
        {
            _totalBagGarbageCount = 0;
            _totalBroomGarbageCount = 0;
            _totalMopGarbageCount = 0;
            _totalPurpleGarbageCount = 0;
            _totalYellowGarbageCount = 0;
        }
#endif

        _counts = new Dictionary<ToolType, int>
        {
            { ToolType.Broom, _totalBroomGarbageCount },
            { ToolType.Mop, _totalMopGarbageCount },
            { ToolType.GarbageBag, _totalBagGarbageCount },
            { ToolType.Purple, _totalPurpleGarbageCount },
            { ToolType.Yellow, _totalYellowGarbageCount },
        };
    }
    public void GarbageCollected(ToolType garbage)
    {
        _counts[garbage]++;
        _totalBagGarbageCount = _counts[ToolType.GarbageBag];
        _totalBroomGarbageCount = _counts[ToolType.Broom];
        _totalMopGarbageCount = _counts[ToolType.Mop];
        _totalPurpleGarbageCount = _counts[ToolType.Purple];
        _totalYellowGarbageCount = _counts[ToolType.Yellow];
    }

}
