using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class ToolBase
{

    public ToolType _type;
    [Tooltip("How many times the player used the tool")]
    public int  _toolUses;
    [Tooltip("How many times the tool can be used")]
    public int _durablity = 5;


    
    

}
public enum ToolType
{
    Broom,
    GarbageBag,
    Mop
}
