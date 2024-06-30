using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class PowerUpBase
{
    public PowerUpType _type;
    [Tooltip("How Long the PowerUp lasts")]
    public float _length = 5f;
    [Tooltip("UI informations")]
    [SerializeField] public PowerUpUI _PowerUpUI;

    [System.NonSerialized] public float _PowerUpTimer; // How long the player has had the PowerUp



}
public enum PowerUpType
{
    Vaccum,
    Speed_Boots,
    Score_Mult
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
