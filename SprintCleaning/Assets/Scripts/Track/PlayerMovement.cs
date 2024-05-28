using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private const int TARGET_POINT_INDEX = 1;
    // ^ every time the player reaches the next point, it creates the next track segment and deletes the oldest one

    //private const float MAX_FRACTION_LANE_CHANGE_SPEED_AT_END = .1f;
    //// ^ when the player is changing lanes with _discreteMovement true, they shouldn't accelerate to max speed and then just stop
    //// instantly. Instead, the maximum lane change speed is reduced based on how far they are, so it's full at half a lane away
    //// from the target lane (or half a lane away from the current lane, if the target lane is multiple lanes away). At the start
    //// lane and target lane, the maximum lane change speed is multiplied by this. Linearly interpolate the max speed.


    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private float _playerSpeed = 0f;
    [SerializeField] private float _laneChangeSpeed = 5f;
    [SerializeField] private float _rotationSpeed = 300;
    [SerializeField] private bool _discreteMovement = false;
    private PlayerMovementTargetLane _targetLaneTracker;

    private TrackPositions _track;

    private TrackGenerator gameManager;


    private void Awake() 
    {
        gameManager = TrackGenerator.Instance;
        _track = TrackPositions.Instance;
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
        float currentLane = _track.ConvertPositionToLane(TARGET_POINT_INDEX, _rigidbody.position);
        if (currentLane == _targetLaneTracker.TargetLane || !_targetLaneTracker.TargetLane.HasValue)
            return;

        Vector3 targetLanePoint = _track.PositionToBeOnLane(TARGET_POINT_INDEX, _targetLaneTracker.TargetLane.Value, _rigidbody.position);

        Vector3 laneChangeVelocity = _laneChangeSpeed * (targetLanePoint - _rigidbody.position).normalized;
        VectorUtils.LimitVelocityToPreventOvershoot(ref laneChangeVelocity, _rigidbody.position, targetLanePoint, Time.deltaTime);

        _rigidbody.velocity += laneChangeVelocity;
    }
}
