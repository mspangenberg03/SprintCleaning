using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerMovementSettings", menuName = "PlayerMovementSettings")]
public class PlayerMovementSettings : ScriptableObject
{
    [SerializeField] public bool _discreteMovement;
    [SerializeField] public float _playerSpeed;
    [SerializeField] public float _maxLaneChangeSpeed;
    [SerializeField] public float _laneChangeSpeedupTime;
    [SerializeField] public float _laneChangeTurnaroundTime;
    [SerializeField] public float _laneChangeStoppingTime;
    [SerializeField] public float _rotationSpeed;
}
