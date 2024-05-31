using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerMovementSettings", menuName = "PlayerMovementSettings")]
public class PlayerMovementSettings : ScriptableObject
{
    [SerializeField] public float PlayerSpeed { get; private set; } = 10;
    [SerializeField] public float MaxLaneChangeSpeed { get; private set; } = 5;
    [SerializeField] public float LaneChangeSpeedupTime { get; private set; } = .05f;
    [SerializeField] public float LaneChangeTurnaroundTime { get; private set; } = .1f;
    [SerializeField] public float LaneChangeStoppingTime { get; private set; } = .15f;
    [SerializeField] public float RotationSpeed { get; private set; } = 300f;

    [SerializeField] public float DistanceBetweenLanes { get; private set; } = 1.5f;
    [SerializeField] public float PlayerVerticalOffset { get; private set; } = 1.5f;
}
