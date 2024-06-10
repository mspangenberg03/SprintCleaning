using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class ToolBase
{
    public GarbageType _type;
    [Tooltip("How many times the tool can be used")]
    public int _durablity = 5;
    [Tooltip("UI informations")]
    [SerializeField] public ToolUI _toolUI;

    [System.NonSerialized] public int _toolUses; // How many times the player used the tool



}
public enum GarbageType
{
    Kick,
    Hat,
    Snare,
    Cymbal,
    TomLow,
    TomMed,
    TomHigh,
}
