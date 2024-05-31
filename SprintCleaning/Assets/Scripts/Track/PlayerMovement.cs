using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private const int TARGET_POINT_INDEX = 1;
    // ^ every time the player reaches the next point, it creates the next track segment and deletes the oldest one, so the player is always
    // moving towards this point index.

    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private PlayerMovementSettings _settings;

    private float _laneChangeSpeed;
    private TrackGenerator gameManager;

    private bool LeftKey => Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
    private bool RightKey => Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);

    private static PlayerMovementSettings _settingsStatic;
    public static PlayerMovementSettings Settings 
    {
        get
        {
            if (_settingsStatic == null)
            {
                _settingsStatic = FindObjectOfType<PlayerMovement>()._settings;
            }
            return _settingsStatic;
        }
    }

    private void Awake() 
    {
        _settingsStatic = _settings;
        gameManager = TrackGenerator.Instance;
    }



    private void Start()
    {
        _rigidbody.position = TrackGenerator.Instance.TrackPieces[0].EndTransform.position + Vector3.up * _settings.PlayerVerticalOffset;
        _rigidbody.transform.position = _rigidbody.position;
        PlayerMovementProcessor.SetFixedDeltaTime();
    }

    private void FixedUpdate()
    {
        TrackPiece trackPiece = TrackGenerator.Instance.TrackPieces[TARGET_POINT_INDEX];

        Vector3 currentPosition = _rigidbody.position - Vector3.up * _settings.PlayerVerticalOffset;

        float t = trackPiece.FindTForClosestPointOnMidline(currentPosition);
        float currentLane = trackPiece.Lane(currentPosition, t);

        trackPiece.StoreLane(currentLane);
        Vector3 trackEnd = trackPiece.EndPosition;

        Vector3 forwardsVelocity = ForwardsVelocity(trackPiece, currentPosition, t, out bool goingStraightTowardsEnd, trackEnd);
        _rigidbody.velocity = forwardsVelocity;

        if (goingStraightTowardsEnd && VectorUtils.VelocityWillOvershoot(forwardsVelocity.To2D().To3D(), currentPosition.To2D().To3D(), trackEnd.To2D().To3D(), Time.deltaTime))
        {
            gameManager.AddTrackPiece();
        }

        _rigidbody.MoveRotation(PlayerMovementProcessor.NextRotation(_settings.RotationSpeed, forwardsVelocity, _rigidbody.rotation));

        _rigidbody.velocity += SidewaysVelocity(currentLane, forwardsVelocity);
    }

    private Vector3 ForwardsVelocity(TrackPiece trackPiece, Vector3 currentPosition, float t, out bool goingStraightTowardsEnd, Vector3 trackEnd)
    {
        // The derivative of the bezier curve is the direction of the velocity, if timesteps were infinitely small.
        // Use the 2nd derivative to help reduce the error from discrete timesteps.
        Vector3 derivative = trackPiece.BezierCurveDerivative(t);
        Vector3 secondDerivative = trackPiece.BezierCurveSecondDerivative();
        float estimatedTChangeDuringTimestep = _settings.PlayerSpeed * Time.deltaTime / derivative.magnitude;
        Vector3 averageDerivative = derivative + estimatedTChangeDuringTimestep / 2 * secondDerivative;
        Vector3 result = _settings.PlayerSpeed * averageDerivative.normalized;

        // The player's y position shifts very slightly even on a flat track. Not sure why, maybe internal physics engine stuff.
        // Do this to keep the y position's drift in check.
        Vector3 point = trackPiece.BezierCurve(t);
        float yDifference = point.y - currentPosition.y;
        result.y += 10f * yDifference * Time.deltaTime;

        goingStraightTowardsEnd = (trackEnd - currentPosition).magnitude <= _settings.PlayerSpeed * Time.deltaTime;
        if (goingStraightTowardsEnd)
        {
            float yResult = result.y;
            result = _settings.PlayerSpeed * (trackEnd - currentPosition).normalized;
            result.y = yResult;
        }

        return result;
    }
    

    private Vector3 SidewaysVelocity(float currentLane, Vector3 forwardsVelocity)
    {
        AccelerateLaneChangeSpeed(currentLane);

        Vector3 laneChangeDirection = -Vector2.Perpendicular(forwardsVelocity.To2D()).normalized.To3D();
        Vector3 laneChangeVelocity = _laneChangeSpeed * laneChangeDirection;

        float nextLane = currentLane + _laneChangeSpeed / _settings.DistanceBetweenLanes * Time.deltaTime;
        if (nextLane < -1f || nextLane > 1f)
        {
            // don't overshoot
            nextLane = nextLane < -1f ? -1f : 1f;
            _laneChangeSpeed = (nextLane - currentLane) * _settings.DistanceBetweenLanes / Time.deltaTime;
            laneChangeVelocity = _laneChangeSpeed * laneChangeDirection;
        }

        return laneChangeVelocity;
    }

    private void AccelerateLaneChangeSpeed(float currentLane)
    {
        // This only changes _laneChangeSpeed. Adjust it more gradually than instantly moving at the maximum lane change speed,
        // to make it feel better.

        float? targetLane = null;
        if (RightKey == LeftKey) // might feel better if remember the most recent one and use that
            targetLane = null;
        else if (RightKey)
            targetLane = 1;
        else if (LeftKey)
            targetLane = -1;

        bool slowDown = currentLane == targetLane || !targetLane.HasValue;

        float accelerationDirection;
        if (slowDown)
            accelerationDirection = -Mathf.Sign(_laneChangeSpeed);
        else
            accelerationDirection = Mathf.Sign(targetLane.Value - currentLane);

        float accelerationTime = _settings.LaneChangeSpeedupTime;
        if (Mathf.Sign(accelerationDirection) != Mathf.Sign(_laneChangeSpeed))
        {
            if (RightKey || LeftKey)
                accelerationTime = _settings.LaneChangeTurnaroundTime;
            else
                accelerationTime = _settings.LaneChangeStoppingTime;
        }

        float laneChangeSpeedSignBefore = Mathf.Sign(_laneChangeSpeed);
        _laneChangeSpeed += _settings.MaxLaneChangeSpeed / accelerationTime * Time.deltaTime * accelerationDirection;
        if (Mathf.Abs(_laneChangeSpeed) > _settings.MaxLaneChangeSpeed)
            _laneChangeSpeed = Mathf.Sign(_laneChangeSpeed) * _settings.MaxLaneChangeSpeed;
        if (slowDown && (laneChangeSpeedSignBefore != Mathf.Sign(_laneChangeSpeed)))
            _laneChangeSpeed = 0;
    }
}
