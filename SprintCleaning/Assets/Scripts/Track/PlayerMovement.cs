using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private const int TARGET_POINT_INDEX = 1;
    // ^ every time the player reaches the next point, it creates the next track segment and deletes the oldest one, so the player is always
    // moving towards this point index.

    //private const float MAX_FRACTION_LANE_CHANGE_SPEED_AT_END = .1f;
    //// ^ when the player is changing lanes with _discreteMovement true, they shouldn't accelerate to max speed and then just stop
    //// instantly. Instead, the maximum lane change speed is reduced based on how far they are, so it's full at half a lane away
    //// from the target lane (or half a lane away from the current lane, if the target lane is multiple lanes away). At the start
    //// lane and target lane, the maximum lane change speed is multiplied by this. Linearly interpolate the max speed.

    [SerializeField] private int _seed = -1;

    [SerializeField] private Rigidbody _rigidbody;

    [SerializeField] private PlayerMovementSettings _settings;

    private float _laneChangeSpeed;
    private PlayerMovementTargetLane _targetLaneTracker;

    private TrackPositions _track;

    private TrackGenerator gameManager;

    private float? TargetLane => _targetLaneTracker.TargetLane;
    public static PlayerMovementSettings Settings { get; private set; }

    public static Rigidbody test;


    private void Awake() 
    {
        if (_seed == -1)
        {
            _seed = Random.Range(int.MinValue, int.MaxValue);
            Debug.Log("RNG seed: " + _seed);
        }
        Random.InitState(_seed);

        test = _rigidbody;
        Settings = _settings;
        gameManager = TrackGenerator.Instance;
        _track = TrackPositions.Instance;
        
        _targetLaneTracker = new PlayerMovementTargetLane(_settings._discreteMovement);
    }

    private void Start()
    {
        StartOnTrack();
        PlayerMovementProcessor.SetFixedDeltaTime();
    }

    private void StartOnTrack()
    {
        _targetLaneTracker.Reset();
        _rigidbody.position = _track.TrackPoint(0);
        _rigidbody.transform.position = _rigidbody.position;
    }

    private void FixedUpdate()
    {
        TrackPiece trackPiece = TrackGenerator.Instance.TrackPieces[TARGET_POINT_INDEX];

        Vector3 currentPosition = _rigidbody.position - Vector3.up * TrackPositions.Instance.PlayerVerticalOffset;
        float currentLane = trackPiece.Lane(currentPosition, out float t);

        Vector3 forwardsVelocity = NextVelocityAlongTrack(trackPiece, currentPosition, currentLane, t, out bool goingStraightTowardsEnd, out Vector3 trackEnd);
        _rigidbody.velocity = forwardsVelocity;

        if (goingStraightTowardsEnd && VectorUtils.VelocityWillOvershoot(forwardsVelocity.To2D().To3D(), currentPosition.To2D().To3D(), trackEnd.To2D().To3D(), Time.deltaTime))
        {
            gameManager.AddTrackPiece();
        }


        _targetLaneTracker.OnFixedUpdate();

        _rigidbody.MoveRotation(PlayerMovementProcessor.NextRotation(_settings._rotationSpeed, forwardsVelocity, _rigidbody.rotation));

        LaneMovement(trackPiece, currentLane, t, forwardsVelocity, currentPosition); // should check whether this is always orthogonal to lane movement. It should be.
    }

    private Vector3 NextVelocityAlongTrack(TrackPiece trackPiece, Vector3 currentPosition, float currentLane
        , float t, out bool goingStraightTowardsEnd, out Vector3 trackEnd)
    {
        trackPiece.StoreLane(currentLane);
        trackEnd = trackPiece.EndPosition;

        // The derivative of the bezier curve is the direction of the velocity, if timesteps were infinitely small.
        // Use the 2nd derivative to help reduce the error from discrete timesteps.
        Vector3 derivative = trackPiece.BezierCurveDerivative(t);
        Vector3 secondDerivative = trackPiece.BezierCurveSecondDerivative();
        float estimatedTChangeDuringTimestep = _settings._playerSpeed * Time.deltaTime / derivative.magnitude;
        Vector3 averageDerivative = derivative + estimatedTChangeDuringTimestep / 2 * secondDerivative;
        Vector3 result = _settings._playerSpeed * averageDerivative.normalized;

        // The player's y position shifts very slightly even on a flat track. Not sure why, maybe internal physics engine stuff.
        // Do this to keep the y position's drift in check.
        Vector3 point = trackPiece.BezierCurve(t);
        float yDifference = point.y - currentPosition.y;
        result.y += 10f * yDifference * Time.deltaTime;

        goingStraightTowardsEnd = (trackEnd - currentPosition).magnitude <= _settings._playerSpeed * Time.deltaTime;
        if (goingStraightTowardsEnd)
        {
            float yResult = result.y;
            result = _settings._playerSpeed * (trackEnd - currentPosition).normalized;
            result.y = yResult;
        }

        return result;
    }

    private void Update()
    {
        _targetLaneTracker.OnUpdate();
    }
    

    private void LaneMovement(TrackPiece trackPiece, float currentLane, float t, Vector3 forwardsVelocity, Vector3 currentPosition)
    {
        AccelerateLaneChangeSpeed(currentLane);

        //// Find a point to the left or right of the player on the lane which the player is moving towards.
        //float laneToGoTowards = Mathf.Sign(_laneChangeSpeed);
        //if (_settings._discreteMovement)
        //    laneToGoTowards = TargetLane.Value;

        Vector3 laneChangeDirection = -Vector2.Perpendicular(forwardsVelocity.To2D()).normalized.To3D();
        Vector3 laneChangeVelocity = _laneChangeSpeed * laneChangeDirection;

        trackPiece.StoreLane(0);

        // The track midpoint is at the intersection of the track's bezier curve and a line along laneChangeVelocity
        Vector2 linePoint1 = currentPosition.To2D();
        Vector2 linePoint2 = linePoint1 + laneChangeDirection.To2D();
        (Vector2, Vector2) possibleMidpoints = trackPiece.IntersectionsOfBezierCurveWithLine2D(linePoint1, linePoint2);
        Vector2 approximateMidpoint = trackPiece.BezierCurve(t);
        Vector2 trackMidpoint;
        if ((approximateMidpoint - possibleMidpoints.Item1).sqrMagnitude < (approximateMidpoint - possibleMidpoints.Item2).sqrMagnitude)
            trackMidpoint = possibleMidpoints.Item1;
        else
            trackMidpoint = possibleMidpoints.Item2;
        Vector3 trackMidpoint3d = trackMidpoint.To3D();
        trackMidpoint3d.y = currentPosition.y;

        Vector3 movingTowards = trackMidpoint3d + Mathf.Sign(_laneChangeSpeed) * laneChangeDirection * TrackPositions.Instance.DistanceBetweenLanes;
        if (VectorUtils.LimitVelocityToPreventOvershoot(ref laneChangeVelocity, currentPosition, movingTowards, Time.deltaTime))
        {
            // It's going to reach the edge of the track. Without this, it takes a moment to move the other direction
            // after reaching the edge of the track.
            _laneChangeSpeed = 0;
        }

        if (laneChangeVelocity.y != 0)
            Debug.Log("laneChangeVelocity y vel: " + laneChangeVelocity.y);

        _rigidbody.velocity += laneChangeVelocity;
    }

    private void AccelerateLaneChangeSpeed(float currentLane)
    {
        // This only changes _laneChangeSpeed. Adjust it more gradually than instantly moving at the maximum lane change speed,
        // to make it feel better.

        bool slowDown = currentLane == TargetLane || !TargetLane.HasValue;

        float accelerationDirection;
        if (slowDown)
            accelerationDirection = -Mathf.Sign(_laneChangeSpeed);
        else
            accelerationDirection = Mathf.Sign(TargetLane.Value - currentLane);

        float accelerationTime = _settings._laneChangeSpeedupTime;
        if (Mathf.Sign(accelerationDirection) != Mathf.Sign(_laneChangeSpeed))
        {
            if (_targetLaneTracker.AnyInput)
                accelerationTime = _settings._laneChangeTurnaroundTime;
            else
                accelerationTime = _settings._laneChangeStoppingTime;
        }

        float laneChangeSpeedSignBefore = Mathf.Sign(_laneChangeSpeed);
        _laneChangeSpeed += _settings._maxLaneChangeSpeed / accelerationTime * Time.deltaTime * accelerationDirection;
        if (Mathf.Abs(_laneChangeSpeed) > _settings._maxLaneChangeSpeed)
            _laneChangeSpeed = Mathf.Sign(_laneChangeSpeed) * _settings._maxLaneChangeSpeed;
        if (slowDown && (laneChangeSpeedSignBefore != Mathf.Sign(_laneChangeSpeed)))
            _laneChangeSpeed = 0;
    }
}
