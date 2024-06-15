using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu]
public class PowerUpUI : ScriptableObject
{
    [SerializeField] private Sprite _sprite;
    public Sprite Sprite { get { return _sprite; } }

    [SerializeField] private PowerUpType _PowerUpType;
    public  PowerUpType PowerUpType{ get { return _PowerUpType; } }
}
