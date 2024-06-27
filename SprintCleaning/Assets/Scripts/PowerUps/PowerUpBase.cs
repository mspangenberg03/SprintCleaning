using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class PowerUpBase
{
    public PowerUpType _type;
    [Tooltip("How many times the PowerUp can be used")]
    public int _durablity = 5;
    [Tooltip("UI informations")]
    [SerializeField] public PowerUpUI _PowerUpUI;

    [System.NonSerialized] public int _PowerUpUses; // How many times the player used the PowerUp



}
public enum PowerUpType
{
    temp
}
public enum GarbageType
{
    TrashCan,
    GlassShard,
    Coke,
    Bottle,
    MilkJug,
    Banana,

    Count
}
