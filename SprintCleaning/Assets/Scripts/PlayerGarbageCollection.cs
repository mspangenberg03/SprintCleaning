using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class PlayerGarbageCollection : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _garbageText;

    [SerializeField]
    private CollectedItems _playerItemData;

    
    private void Start()
    {
        _playerItemData.Awake();
    }
    public void TextEdit()
    {
        if (_garbageText != null)
        {
            string text = "";
            for (int i = 0; i < (int)(GarbageType.Count); i++)
                text += (GarbageType)i + ": " + _playerItemData._counts[(GarbageType)i] + "\n";

            _garbageText.text = text;
        }

    }



}
