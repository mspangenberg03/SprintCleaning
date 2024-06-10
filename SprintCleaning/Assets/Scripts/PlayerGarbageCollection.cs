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
            _garbageText.text = "Broom Garbage: " + _playerItemData._counts[GarbageType.Kick]
                + "\n" + "Mop Garbage: " + _playerItemData._counts[GarbageType.Snare]
                + "\n" + "Bag Garbage: " + _playerItemData._counts[GarbageType.Hat]
                + "\n" + "Purple Garbage: " + _playerItemData._counts[GarbageType.Cymbal]
                ;
        }

    }



}
