﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[DefaultExecutionOrder(-10)]
public class PlayerMovement : MonoBehaviour
{
    private const int TARGET_POINT_INDEX = 1;
    // ^ every time the player reaches the next point, it creates the next track segment and deletes the oldest one, so the player is always
    // moving towards this point index.

    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private PlayerMovementSettings _settings;
    [SerializeField] private Animator _animator;
    private float _currentTargetLane;
    private bool _changingLanes;
    
    private float _jumpInputTime = float.NegativeInfinity;
    private TrackGenerator _trackGenerator;

    // input polling (in case of frames w/o fixed update)
    private bool _polledInputThisFrame;
    private bool _leftInputDown;
    private bool _rightInputDown;
    private bool _jumpInput;

    // position & velocity

    private Vector3 _positionOnMidline;

    private float _lanePosition;
    private float _laneChangeSpeed;

    private float _jumpPosition;
    private float _jumpSpeed;

    private float CurrentForwardsSpeed => _settings.BaseForwardsSpeed * (1f - Game_Over.Instance.FractionOfGameOverDelayElapsed);

    private bool LeftInputDown => !Game_Over.Instance.GameIsOver && (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow));
    private bool RightInputDown => !Game_Over.Instance.GameIsOver && (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow));
    private bool JumpInput => !Game_Over.Instance.GameIsOver && Input.GetKey(KeyCode.Space);

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
        _trackGenerator = TrackGenerator.Instance;
    }



    private void Start()
    {
        _positionOnMidline = _trackGenerator.TrackPieces[0].EndTransform.position + Vector3.up * _settings.PlayerVerticalOffset;
        _rigidbody.position = _positionOnMidline;
        _rigidbody.transform.position = _rigidbody.position;
        _animator.SetFloat("Speed", 2f);
    }

    private void Update()
    {
        PollInputsOncePerFrame();
        _polledInputThisFrame = false;
    }

    private void FixedUpdate()
    {
        System.Threading.Thread.MemoryBarrier(); // Just in case. Probably don't need this but it might make dspTime more up to date.
        if (AudioSettings.dspTime < GameplayMusic.Instance.AudioStartTime)
        {
            return;
        }

        PollInputsOncePerFrame();
        DevHelper.Instance.GameplayReproducer.StartNextFixedUpdate();
        DevHelper.Instance.GameplayReproducer.SaveOrLoadMovementInputs(ref _leftInputDown, ref _rightInputDown, ref _jumpInput);

        TrackPiece trackPiece = _trackGenerator.TrackPieces[TARGET_POINT_INDEX];
        float t = trackPiece.FindTForClosestPointOnMidline(_positionOnMidline);

        if (DevHelper.Instance.LogAudioTimeAndPlayerProgressAlongTrack)
            Debug.Log("t (meaningless unless near 0 or 1) & audio time (doesn't update constantly): " + t + " " + GameplayMusic.CurrentAudioTime);


        Vector3 midlineVelocity = TrackMidlineVelocity(trackPiece, t);

        Vector3 priorPositionOnMidline = _positionOnMidline; 
        _positionOnMidline += midlineVelocity * Time.deltaTime;

        UpdateJumpPosition();

        UpdateLanePosition();

        _rigidbody.MoveRotation(PlayerMovementProcessor.NextRotation(_settings.RotationSpeed, midlineVelocity, _rigidbody.rotation));

        trackPiece.StoreLane(0);
        Vector3 approximatedPositionOnMidline = trackPiece.BezierCurve(t);
        trackPiece.StoreLane(_lanePosition);
        Vector3 approximatedPositionAtLanePosition = trackPiece.BezierCurve(t);
        Vector3 offsetForLanePosition = approximatedPositionAtLanePosition - approximatedPositionOnMidline;

        Vector3 currentPosition = _positionOnMidline + offsetForLanePosition + _jumpPosition * Vector3.up;

        _rigidbody.velocity = (currentPosition - _rigidbody.position) / Time.deltaTime;


        trackPiece.StoreLane(0);
        Vector3 endPosition = trackPiece.BezierCurve(1f);
        Vector3 endDirection = trackPiece.BezierCurveDerivative(1f);
        endDirection.y = 0;
        if (VectorUtils.TwoPointsAreOnDifferentSidesOfPlane(priorPositionOnMidline, _positionOnMidline, endPosition, endDirection))
        {
            _trackGenerator.AddTrackPieceAndObjects();
        }
        _trackGenerator.AfterPlayerMovementFixedUpdate();
    }

    private void PollInputsOncePerFrame()
    {
        if (_polledInputThisFrame)
            return;
        _polledInputThisFrame = true;

        if (LeftInputDown)
            _leftInputDown = true;
        if (RightInputDown)
            _rightInputDown = true;
        if (JumpInput)
            _jumpInput = true;
    }


    private Vector3 TrackMidlineVelocity(TrackPiece trackPiece, float t)
    {
        trackPiece.StoreLane(0);
        // The derivative of the bezier curve is the direction of the velocity, if timesteps were infinitely small.
        // Use the 2nd derivative to help reduce the error from discrete timesteps.
        Vector3 derivative = trackPiece.BezierCurveDerivative(t);
        Vector3 secondDerivative = trackPiece.BezierCurveSecondDerivative();
        float estimatedTChangeDuringTimestep = CurrentForwardsSpeed * Time.deltaTime / derivative.magnitude;
        Vector3 averageDerivative = derivative + estimatedTChangeDuringTimestep / 2 * secondDerivative;
        Vector3 result = CurrentForwardsSpeed * averageDerivative.normalized;

        // The y position shifts off the track near the transition between track pieces, so correct for that.
        // Only need to deal with the y position offset here because the lane movement deals with x and z drift.
        Vector3 point = trackPiece.BezierCurve(t) + _settings.PlayerVerticalOffset * Vector3.up;
        float yDifference = point.y - _positionOnMidline.y;
        result.y += yDifference / Time.deltaTime;

        return result;
    }

    #region Lane Changing
    private void UpdateLanePosition()
    {
        CheckLaneChangeInputs();

        AccelerateLaneChangeSpeed(_lanePosition);

        float nextLanePosition = _lanePosition + _laneChangeSpeed / _settings.DistanceBetweenLanes * Time.deltaTime;
        if (Mathf.Sign(nextLanePosition - _currentTargetLane) != Mathf.Sign(_lanePosition - _currentTargetLane) || _lanePosition == _currentTargetLane)
        {
            // don't overshoot
            _changingLanes = false;
            CheckLaneChangeInputs();
            if (!_changingLanes)
            {
                nextLanePosition = _currentTargetLane;
                _laneChangeSpeed = 0;
            }
        }
        _lanePosition = nextLanePosition;

    }

    private void CheckLaneChangeInputs()
    {
        float priorTargetLane = _currentTargetLane;

        if (_leftInputDown)
            _currentTargetLane--;

        if (_rightInputDown)
            _currentTargetLane++;

        _currentTargetLane = Mathf.Clamp(_currentTargetLane, -1, 1);
        if (_currentTargetLane != priorTargetLane)
            _changingLanes = true;
        _leftInputDown = false;
        _rightInputDown = false;
    }

    private void AccelerateLaneChangeSpeed(float currentLane)
    {
        // This only changes _laneChangeSpeed. Adjust it more gradually than instantly moving at the maximum lane change speed,
        // to make it feel better. It still instantly stops upon reaching the target lane.

        float accelerationDirection = Mathf.Sign(_currentTargetLane - currentLane);

        float accelerationTime = _settings.LaneChangeSpeedupTime;
        if (Mathf.Sign(accelerationDirection) != Mathf.Sign(_laneChangeSpeed))
            accelerationTime = _settings.LaneChangeTurnaroundTime;

        _laneChangeSpeed += _settings.BaseLaneChangeSpeed / accelerationTime * Time.deltaTime * accelerationDirection;
        if (Mathf.Abs(_laneChangeSpeed) > _settings.BaseLaneChangeSpeed)
            _laneChangeSpeed = Mathf.Sign(_laneChangeSpeed) * _settings.BaseLaneChangeSpeed;
    }
    #endregion


    #region Jumping
    private void UpdateJumpPosition()
    {
        // The jump position is relative to the midline position, so make it so the player doesn't move upwards/downwards if 
        // the track goes upwards/downwards while the player is jumping.
        if (_jumpPosition > 0)
            CheckJumpHitsGround();

        if (_jumpInput)
        {
            _jumpInputTime = Time.time;
            _jumpInput = false;
        }

        bool executeJump = (Time.time <= _jumpInputTime + _settings.JumpBufferDuration) && _jumpPosition == 0;
        if (executeJump)
        {
            _animator.SetTrigger("Jump");
            _jumpSpeed = _settings.JumpUpDuration * _settings.GravityAccelerationWhileRising;
            _jumpInputTime = float.NegativeInfinity;
        }

        if (_jumpPosition > 0 || executeJump)
        {
            float gravity = _jumpSpeed >= 0 ? _settings.GravityAccelerationWhileRising : _settings.GravityAccelerationWhileFalling;
            if (executeJump)
                gravity /= 2;// This seems to be necessary to make the jump height correct.
            _jumpSpeed -= gravity * Time.deltaTime;
        }
        _jumpPosition += _jumpSpeed * Time.deltaTime;
        CheckJumpHitsGround();
    }

    private void CheckJumpHitsGround()
    {
        if (_jumpPosition <= 0)
        {
            _jumpPosition = 0;
            _jumpSpeed = Mathf.Max(0, _jumpSpeed);
        }
    }


    #endregion

    
}
