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
    public Dictionary<GarbageType, int> _counts;
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

        _counts = new Dictionary<GarbageType, int>
        {
            { GarbageType.Kick, _totalBroomGarbageCount },
            { GarbageType.Snare, _totalMopGarbageCount },
            { GarbageType.Hat, _totalBagGarbageCount },
            { GarbageType.Cymbal, _totalPurpleGarbageCount },
            { GarbageType.TomHigh, _totalYellowGarbageCount },
        };
    }
    public void GarbageCollected(GarbageType garbage)
    {
        _counts[garbage]++;
        _totalBagGarbageCount = _counts[GarbageType.Hat];
        _totalBroomGarbageCount = _counts[GarbageType.Kick];
        _totalMopGarbageCount = _counts[GarbageType.Snare];
        _totalPurpleGarbageCount = _counts[GarbageType.Cymbal];
        _totalYellowGarbageCount = _counts[GarbageType.TomHigh];
    }

}
