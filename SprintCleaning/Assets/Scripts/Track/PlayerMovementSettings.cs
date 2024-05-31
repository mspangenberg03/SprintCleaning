using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerMovementSettings", menuName = "PlayerMovementSettings")]
public class PlayerMovementSettings : ScriptableObject
{
    [field: SerializeField] public float PlayerSpeed { get; private set; } = 10;
    [field: SerializeField] public float MaxLaneChangeSpeed { get; private set; } = 5;
    [field: SerializeField] public float LaneChangeSpeedupTime { get; private set; } = .05f;
    [field: SerializeField] public float LaneChangeTurnaroundTime { get; private set; } = .1f;
    [field: SerializeField] public float LaneChangeStoppingTime { get; private set; } = .15f;
    [field: SerializeField] public float RotationSpeed { get; private set; } = 300f;

    [field: SerializeField] public float DistanceBetweenLanes { get; private set; } = 1.5f;
    [field: SerializeField] public float PlayerVerticalOffset { get; private set; } = 1.5f;
}
