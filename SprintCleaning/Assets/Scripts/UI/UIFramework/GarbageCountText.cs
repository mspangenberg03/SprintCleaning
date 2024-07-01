using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

public class GarbageCountText : CustomText
{
    [SerializeField]
    private TextMeshProUGUI _garbageCountText;
    [SerializeField]
    private DataManager _dataManager => DataManager.Instance;
    private void Start()
    {
        if (_dataManager == null)
            return;

        string text = "";

        for(int i = 0; i < _dataManager._garbageConvert.Count; i++)
        {
            text = text + '\n' + _dataManager._garbageConvert[(GarbageType)i] + ':' + ScoreManager._counts[(GarbageType)i] + '\n';
        }
        _garbageCountText.text = "Garbage Collected" + '\n' + text;
    }
}
