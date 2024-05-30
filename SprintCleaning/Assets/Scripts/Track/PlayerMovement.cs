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
    private PlayerMovementTargetLane _targetLaneTracker;

    private TrackPositions _track;

    private TrackGenerator gameManager;

    private float? TargetLane => _targetLaneTracker.TargetLane;
    public static PlayerMovementSettings Settings { get; private set; }

    public static Rigidbody test;


    private void Awake() 
    {
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

        Vector3 nextVelocity = NextVelocityAlongTrack(trackPiece, currentPosition, currentLane, t, out bool goingStraightTowardsEnd, out Vector3 trackEnd);
        _rigidbody.velocity = nextVelocity;

        if (goingStraightTowardsEnd && VectorUtils.VelocityWillOvershoot(_rigidbody.velocity.To2D().To3D(), _rigidbody.position.To2D().To3D(), trackEnd.To2D().To3D(), Time.deltaTime))
        {
            //Debug.Log("add track piece");
            gameManager.AddTrackPiece();
        }


        _targetLaneTracker.OnFixedUpdate();

        _rigidbody.MoveRotation(PlayerMovementProcessor.NextRotation(_settings._rotationSpeed, nextVelocity, _rigidbody.rotation));

        LaneMovement(trackPiece, currentLane);
    }

    private Vector3 NextVelocityAlongTrack(TrackPiece trackPiece, Vector3 currentPosition, float currentLane
        , float t, out bool goingStraightTowardsEnd, out Vector3 trackEnd)
    {
        trackPiece.StoreLane(currentLane);
        trackEnd = trackPiece.EndPosition;

        if ((trackEnd - currentPosition).magnitude <= _settings._playerSpeed * Time.deltaTime)
        {
            goingStraightTowardsEnd = true;
            return _settings._playerSpeed * (trackEnd - currentPosition).normalized;
        }
        goingStraightTowardsEnd = false;

        // The derivative of the bezier curve is the direction of the velocity, if timesteps were infinitely small.
        // Could use the 2nd derivative to help account for the discrete timesteps.
        return _settings._playerSpeed * trackPiece.BezierCurveDerivative(t).normalized;



        //Vector3 targetPosition = trackPiece.PointToMoveTowardsOnSameLane(currentPosition, _settings._playerSpeed * Time.deltaTime, out goingStraightTowardsEnd, trackEnd, currentLane
        //    , t);

        //return _settings._playerSpeed * (targetPosition - currentPosition).normalized;
    }

    private void Update()
    {
        _targetLaneTracker.OnUpdate();
    }
    

    private void LaneMovement(TrackPiece trackPiece, float currentLane)
    {
        AccelerateLaneChangeSpeed(currentLane);

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

    private void AccelerateLaneChangeSpeed(float currentLane)
    {
        // This only changes _laneChangeSpeed. Adjust it more gradually than instantly moving at the maximum lane change speed,
        // to make it feel better.

        //float currentLane = _track.ConvertPositionToLane(TARGET_POINT_INDEX, _rigidbody.position);

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
