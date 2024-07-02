using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GarbageCountText : CustomText
{
    [SerializeField]
    private TextMeshProUGUI _garbageCountText;
    private DataManager _dataManager;
    private void Start()
    {
        _dataManager = DataManager.Instance;
        if (_dataManager == null)
            return;

        string text = "";

        for(int i = 0; i < _dataManager._garbageConvert.Count; i++)
        {
            text = text + '\n' + _dataManager._garbageConvert[(GarbageType)i] + ':' + ScoreManager._countsForEndScreen[(GarbageType)i] + '\n';
        }
        _garbageCountText.text = "Garbage Collected" + '\n' + text;
    }

    public void UpdateData()
    {
        if (_dataManager == null)
            return;

        string text = "";

        for (int i = 0; i < _dataManager._garbageConvert.Count; i++)
        {
            text = text + '\n' + _dataManager._garbageConvert[(GarbageType)i] + ':' + ScoreManager._countsForEndScreen[(GarbageType)i] + '\n';
        }
        _garbageCountText.text = "Garbage Collected" + '\n' + text;
    }
}
