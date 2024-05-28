using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu]
public class CollectedItems : ScriptableObject
{
    //The garbage collected in a run 
    private int _broomGarbageCount = 0;
    private int _mopGarbageCount = 0;
    private int _bagGarbageCount = 0;
    //The garbage that the player has in total
    private int _totalBagGarbageCount = 0;
    private int _totalBroomGarbageCount = 0;
    private int _totalMopGarbageCount = 0;

    public void NewRun()
    {
        _bagGarbageCount = 0;
        _broomGarbageCount = 0;
        _mopGarbageCount = 0;
    }
    public void TextEdit()
    {
        GameObject player = GameObject.Find("Player");
        TextMeshProUGUI text = player.GetComponent<PlayerGarbageCollection>()._garbageText;
        if(text != null)
        {
            text.text = "Broom Garbage: " + _broomGarbageCount +"\n" +
                        "Mop Garbage: " + _mopGarbageCount +"\n" +
                        "Bag Garbage: " + _bagGarbageCount;
        }
       
        
    }
    public void GarbageCollected(string garbage)
    {
        
        if(garbage == "Broom")
        {
            _broomGarbageCount++;
            _totalBroomGarbageCount++;
        }
        if(garbage == "GarbageBag")
        {
            _bagGarbageCount++;
            _totalBagGarbageCount++;
        }
        if(garbage == "Mop")
        {
            _mopGarbageCount++;
            _totalMopGarbageCount++;
        }
    }
}
