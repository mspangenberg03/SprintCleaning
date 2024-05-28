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
    private bool _discreteMovement = false;
    private float _playerSpeed = 0f;
    private float _maxLaneChangeSpeed = 5f;
    private float _laneChangeSpeedupTime = .1f;
    private float _laneChangeTurnaroundTime = .1f;
    private float _laneChangeStoppingTime = .2f;
    private float _rotationSpeed = 300;

    private float _laneChangeSpeed;
    private PlayerMovementTargetLane _targetLaneTracker;

    private TrackPositions _track;

    private TrackGenerator gameManager;

    private float? TargetLane => _targetLaneTracker.TargetLane;


    private void Awake() 
    {
        gameManager = TrackGenerator.Instance;
        _track = TrackPositions.Instance;

        _discreteMovement = _settings._discreteMovement;
        _playerSpeed = _settings._playerSpeed;
        _maxLaneChangeSpeed = _settings._maxLaneChangeSpeed;
        _laneChangeSpeedupTime = _settings._laneChangeSpeedupTime;
        _laneChangeTurnaroundTime = _settings._laneChangeTurnaroundTime;
        _laneChangeStoppingTime = _settings._laneChangeStoppingTime;  
        _rotationSpeed = _settings._rotationSpeed;
        
        _targetLaneTracker = new PlayerMovementTargetLane(_discreteMovement);
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
        _rigidbody.velocity = directionToNextPoint * _playerSpeed;

        if (VectorUtils.VelocityWillOvershoot(_rigidbody.velocity, _rigidbody.position, EndPoint(), Time.deltaTime))
        {
            gameManager.AddTrackPiece();
        }


        _targetLaneTracker.OnFixedUpdate();
        LaneMovement();

        _rigidbody.MoveRotation(PlayerMovementProcessor.NextRotation(_rotationSpeed, directionToNextPoint, _rigidbody.rotation));
    }

    private void Update()
    {
        _targetLaneTracker.OnUpdate();
    }

    private Vector3 EndPoint()
    {
        return _track.LanePoint(TARGET_POINT_INDEX, _track.ConvertPositionToLane(TARGET_POINT_INDEX, _rigidbody.position));
    }
    

    private void LaneMovement()
    {
        AccelerateLaneChangeSpeed();

        // Find a point to the left or right of the player on the lane which the player is moving towards.
        float laneToGoTowards = Mathf.Sign(_laneChangeSpeed);
        if (_discreteMovement)
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
        float currentLane = _track.ConvertPositionToLane(TARGET_POINT_INDEX, _rigidbody.position);

        bool slowDown = currentLane == TargetLane || !TargetLane.HasValue;

        float accelerationDirection;
        if (slowDown)
            accelerationDirection = -Mathf.Sign(_laneChangeSpeed);
        else
            accelerationDirection = Mathf.Sign(TargetLane.Value - currentLane);

        float accelerationTime = _laneChangeSpeedupTime;
        if (Mathf.Sign(accelerationDirection) != Mathf.Sign(_laneChangeSpeed))
        {
            if (_targetLaneTracker.AnyInput)
                accelerationTime = _laneChangeTurnaroundTime;
            else
                accelerationTime = _laneChangeStoppingTime;
        }

        float laneChangeSpeedSignBefore = Mathf.Sign(_laneChangeSpeed);
        _laneChangeSpeed += _maxLaneChangeSpeed / accelerationTime * Time.deltaTime * accelerationDirection;
        if (Mathf.Abs(_laneChangeSpeed) > _maxLaneChangeSpeed)
            _laneChangeSpeed = Mathf.Sign(_laneChangeSpeed) * _maxLaneChangeSpeed;
        if (slowDown && (laneChangeSpeedSignBefore != Mathf.Sign(_laneChangeSpeed)))
            _laneChangeSpeed = 0;
    }
}
