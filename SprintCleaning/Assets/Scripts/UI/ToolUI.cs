using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu]
public class ToolUI : ScriptableObject
{
    [SerializeField] private Sprite _sprite;
    public Sprite Sprite { get { return _sprite; } }

    [SerializeField] private ToolType _toolType;
    public ToolType ToolType { get { return _toolType; } }
}
