using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerMovementSettings", menuName = "PlayerMovementSettings")]
public class PlayerMovementSettings : ScriptableObject
{
    [field: SerializeField] public float BaseForwardsSpeed { get; private set; } = 10;
    [field: SerializeField] public float MinForwardsSpeed { get; private set; } = 10;
    [field: SerializeField] public float MaxForwardsSpeed { get; private set; } = 30;
    [field: SerializeField] public float ForwardsAcceleration { get; private set; } = 1f; 
    [field: SerializeField] public float ForwardsAccelerationWhileBelowBaseSpeed { get; private set; } = 1f;
    [field: SerializeField] public float AccelerationPauseAfterGarbageSlowdown { get; private set; } = 3f; // seconds 
    // ^ this also applies to lane change speed up to a point. Might be better to asymptotically approach an absolute maximum, or maybe logarithmic so
    // it keeps accelerating forever but slows down over time
    [field: SerializeField] public float BaseLaneChangeSpeed { get; private set; } = 6; // the sideways speed, except for the speedup time
    [field: SerializeField] public float LaneChangeSpeedCap { get; private set; } = 10; // need this cap because above some value, you instantly go to the opposite side
    [field: SerializeField] public float LaneChangeSpeedupTime { get; private set; } = .05f;
    [field: SerializeField] public float LaneChangeTurnaroundTime { get; private set; } = .1f;
    [field: SerializeField] public float LaneChangeStoppingTime { get; private set; } = .15f;
    [field: SerializeField] public float RotationSpeed { get; private set; } = 300f;

    [field: SerializeField] public float DistanceBetweenLanes { get; private set; } = 1.5f;
    [field: SerializeField] public float PlayerVerticalOffset { get; private set; } = 1.5f;
}
