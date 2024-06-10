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
    public Dictionary<GarbageType, int> _counts;
    public void Awake()
    {
        if (_counts == null)
        {
            _counts = new Dictionary<GarbageType, int>();
            for (int i = 0; i < (int)(GarbageType.Count); i++)
                _counts.Add((GarbageType)i, 0);
        }

#if UNITY_EDITOR
        if (_resetInEditorOnAwake)
        {
            for (int i = 0; i < (int)(GarbageType.Count); i++)
                _counts[(GarbageType)i] = 0;
        }
#endif
    }
    public void GarbageCollected(GarbageType garbage)
    {
        _counts[garbage]++;
    }

}
