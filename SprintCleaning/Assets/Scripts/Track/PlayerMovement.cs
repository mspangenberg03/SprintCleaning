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


    [SerializeField] private Rigidbody _rigidbody;

    [SerializeField] private PlayerMovementSettings _settings;

    private float _laneChangeSpeed;
    private Vector3 _priorVelocityAlongTrack = new Vector3(float.NaN, 0, 0);
    private PlayerMovementTargetLane _targetLaneTracker;

    private TrackPositions _track;

    private TrackGenerator gameManager;

    private float? TargetLane => _targetLaneTracker.TargetLane;
    public static PlayerMovementSettings Settings { get; private set; }


    private void Awake() 
    {
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

        Vector3 nextVelocity = NextVelocityAlongTrack(trackPiece, out bool goingStraightTowardsEnd);
        _priorVelocityAlongTrack = nextVelocity;
        _rigidbody.velocity = nextVelocity;

        if (goingStraightTowardsEnd && VectorUtils.VelocityWillOvershoot(_rigidbody.velocity, _rigidbody.position, EndPoint(), Time.deltaTime))
        {
            gameManager.AddTrackPiece();
        }


        _targetLaneTracker.OnFixedUpdate();

        _rigidbody.MoveRotation(PlayerMovementProcessor.NextRotation(_settings._rotationSpeed, nextVelocity, _rigidbody.rotation));

        LaneMovement(trackPiece);
    }

    private Vector3 NextVelocityAlongTrack(TrackPiece trackPiece, out bool goingStraightTowardsEnd)
    {

        // Travel along a bezier curve to smoothly travel along the current track piece.
        // https://en.wikipedia.org/wiki/B%C3%A9zier_curve#Quadratic_B%C3%A9zier_curves

        float currentLane = _track.ConvertPositionToLane(TARGET_POINT_INDEX, _rigidbody.position); // bugged (see the method. might need to use line segments)
        trackPiece.StoreLane(currentLane);

        Vector3 currentPosition = _rigidbody.position - Vector3.up * TrackPositions.Instance.PlayerVerticalOffset;

        float k = _settings._playerSpeed * Time.deltaTime; // distance the player will travel

        goingStraightTowardsEnd = (currentPosition - trackPiece.EndPosition).magnitude < 1.25f * k;
        if (goingStraightTowardsEnd)
        {
            return VelocityToDirectlyGoToEnd();
        }

        Vector3 targetPoint = trackPiece.PointToMoveTowardsOnSameLane(currentPosition, _settings._playerSpeed * Time.deltaTime);
        return _settings._playerSpeed * (targetPoint - currentPosition).normalized;

        //Vector3 r = currentPosition;


        //// B(t) = point on bezier curve, where t is 0 to 1
        //// k = magnitude(r - B(t))

        //// find t such that
        //// 0 = magnitude(r - B(t)) - k = error (the difference between the distance and correct distance the player will travel)
        //// and not the opposite direction of velocity
        //const int steps = 1000;
        //float bestT = 0;
        //float bestError = float.PositiveInfinity;
        //for (int i = 0; i < steps; i++)
        //{
        //    Vector3 BMinusR = trackPiece.BezierCurve((float)i / steps) - r;

        //    float error = Mathf.Abs(BMinusR.magnitude - k);

        //    if (Vector3.Dot(_priorVelocityAlongTrack, BMinusR) >= 0 || float.IsNaN(_priorVelocityAlongTrack.x))
        //    {
        //        if (error < bestError)
        //        {
        //            bestT = (float)i / steps;
        //            bestError = error;
        //        }
        //    }
        //}

        //if (bestError == float.PositiveInfinity)
        //{
        //    goingStraightTowardsEnd = true;
        //    return VelocityToDirectlyGoToEnd();
        //}

        //Vector3 targetPoint = trackPiece.BezierCurve(bestT);

        //Debug.Log($"targetPoint: {targetPoint.DetailedString()}, position: {r.DetailedString()}, _priorVelocityAlongTrack: {_priorVelocityAlongTrack.DetailedString()}");

        //return _settings._playerSpeed * (targetPoint - r).normalized;

    }

    private Vector3 VelocityToDirectlyGoToEnd()
    {
        // go directly towards the end point
        Vector3 directionToNextPoint = (EndPoint() - _rigidbody.position).normalized;
        return directionToNextPoint * _settings._playerSpeed;
    }

    private void Update()
    {
        _targetLaneTracker.OnUpdate();
    }

    private Vector3 EndPoint()
    {
        float currentLane = _track.ConvertPositionToLane(TARGET_POINT_INDEX, _rigidbody.position);
        return _track.LanePoint(TARGET_POINT_INDEX, currentLane);
    }
    

    private void LaneMovement(TrackPiece trackPiece)
    {
        AccelerateLaneChangeSpeed();

        // Find a point to the left or right of the player on the lane which the player is moving towards.
        float laneToGoTowards = Mathf.Sign(_laneChangeSpeed);
        if (_settings._discreteMovement)
            laneToGoTowards = TargetLane.Value;

        trackPiece.StoreLane(laneToGoTowards);
        Vector3 lanePoint = trackPiece.ClosestPointOnStoredLane(_rigidbody.position);
        lanePoint.y = _rigidbody.position.y;

        Vector3 laneChangeVelocity = Mathf.Abs(_laneChangeSpeed) * (lanePoint - _rigidbody.position).normalized;
        if (VectorUtils.LimitVelocityToPreventOvershoot(ref laneChangeVelocity, _rigidbody.position, lanePoint, Time.deltaTime))
        {
            // It's going to reach the edge of the track. Without this, it takes a moment to move the other direction
            // after reaching the edge of the track.
            _laneChangeSpeed = 0;
        }

        _rigidbody.velocity += laneChangeVelocity;
    }

    private void AccelerateLaneChangeSpeed()
    {
        // This only changes _laneChangeSpeed. Adjust it more gradually than instantly moving at the maximum lane change speed,
        // to make it feel better.

        float currentLane = _track.ConvertPositionToLane(TARGET_POINT_INDEX, _rigidbody.position);

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
