using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerMovementSettings", menuName = "PlayerMovementSettings")]
public class PlayerMovementSettings : ScriptableObject
{
    [field: Header("Sideways Movement")]
    [field: SerializeField] public float BaseLaneChangeSpeed { get; private set; } = 6;
    [field: SerializeField] public float LaneChangeSpeedupTime { get; private set; } = .05f;
    [field: SerializeField] public float LaneChangeTurnaroundTime { get; private set; } = .1f;

    [field: Header("Jump Movement")]
    [field: SerializeField] public float JumpHeight { get; private set; } = 3f;
    [field: SerializeField] public float JumpUpDuration { get; private set; } = 1f;
    [field: SerializeField] public float JumpDownDuration { get; private set; } = 1f;
    [field: SerializeField] public float JumpBufferDuration { get; private set; } = .1f;

    [field: Header("Other")]
    [field: SerializeField] public float RotationSpeed { get; private set; } = 300f;
    [field: SerializeField] public float DistanceBetweenLanes { get; private set; } = 1.5f;
    [field: SerializeField] public float PlayerVerticalOffset { get; private set; } = 1.5f;

    public float GravityAccelerationWhileRising => 2f * JumpHeight / (JumpUpDuration * JumpUpDuration);
    public float GravityAccelerationWhileFalling => 2f * JumpHeight / (JumpDownDuration * JumpDownDuration);

}
