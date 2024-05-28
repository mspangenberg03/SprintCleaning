using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private const int TARGET_POINT_INDEX = 1;
    // ^ every time the player reaches the next point, it creates the next track segment and deletes the oldest one, so the player is always
    // moving towards this point index.

    private const float MAX_FRACTION_LANE_CHANGE_SPEED_AT_END = .1f;
    // ^ when the player is changing lanes with _discreteMovement true, they shouldn't accelerate to max speed and then just stop
    // instantly. Instead, the maximum lane change speed is reduced based on how far they are, so it's full at half a lane away
    // from the target lane (or half a lane away from the current lane, if the target lane is multiple lanes away). At the start
    // lane and target lane, the maximum lane change speed is multiplied by this. Linearly interpolate the max speed.


    [SerializeField] private Rigidbody _rigidbody;

    [SerializeField] private PlayerMovementSettings _settings;

    private float _laneChangeSpeed;
    private PlayerMovementTargetLane _targetLaneTracker;

    private TrackPositions _track;

    private TrackGenerator gameManager;

    private float? TargetLane => _targetLaneTracker.TargetLane;


    private void Awake() 
    {
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
        Vector3 directionToNextPoint = (EndPoint() - _rigidbody.position).normalized;
        _rigidbody.velocity = directionToNextPoint * _settings._playerSpeed;

        if (VectorUtils.VelocityWillOvershoot(_rigidbody.velocity, _rigidbody.position, EndPoint(), Time.deltaTime))
        {
            gameManager.AddTrackPiece();
        }


        _targetLaneTracker.OnFixedUpdate();

        _rigidbody.MoveRotation(PlayerMovementProcessor.NextRotation(_settings._rotationSpeed, directionToNextPoint, _rigidbody.rotation));

        LaneMovement();
    }

    private void Update()
    {
        _targetLaneTracker.OnUpdate();
    }

    private Vector3 EndPoint()
    {
        return PlayerLanePoint(TARGET_POINT_INDEX);
    }

    private Vector3 DirectionFromEndPoint()
    {
        return (PlayerLanePoint(TARGET_POINT_INDEX + 1) - EndPoint()).normalized;
    }

    // returns a point on the track at the player's current lane, at an index of the list of points which define the track.
    private Vector3 PlayerLanePoint(int trackIndex)
    {
        float currentLane = _track.ConvertPositionToLane(TARGET_POINT_INDEX, _rigidbody.position);
        return _track.LanePoint(trackIndex, currentLane);
    }
    

    private void LaneMovement()
    {
        AccelerateLaneChangeSpeed();

        // Find a point to the left or right of the player on the lane which the player is moving towards.
        float laneToGoTowards = Mathf.Sign(_laneChangeSpeed);
        if (_settings._discreteMovement)
            laneToGoTowards = TargetLane.Value;
        Vector3 lanePoint = _track.PositionToBeOnLane(TARGET_POINT_INDEX, laneToGoTowards, _rigidbody.position);

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
