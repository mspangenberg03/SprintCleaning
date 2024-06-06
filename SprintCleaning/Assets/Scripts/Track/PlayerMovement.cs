using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    private const int TARGET_POINT_INDEX = 1;
    // ^ every time the player reaches the next point, it creates the next track segment and deletes the oldest one, so the player is always
    // moving towards this point index.

    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private PlayerMovementSettings _settings;
    [SerializeField] private TextMeshProUGUI _speedText;

    private bool _updatedTargetLaneThisFrame;
    private Vector3 _positionOnMidline;
    private float _lanePosition;
    private float _laneChangeSpeed;
    private float _speedMultiplier = 1f;
    private float _lastGarbageSlowdownTime = float.NegativeInfinity;
    private float _currentTargetLane;
    private TrackGenerator gameManager;


    private float CurrentForwardsSpeed
    {
        get => _settings.BaseForwardsSpeed * _speedMultiplier;
        set => _speedMultiplier = Mathf.Clamp(value, _settings.MinForwardsSpeed, _settings.MaxForwardsSpeed) / _settings.BaseForwardsSpeed;
    }

    private bool LeftKey => Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow);
    private bool RightKey => Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow);

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
        _positionOnMidline = TrackGenerator.Instance.TrackPieces[0].EndTransform.position + Vector3.up * _settings.PlayerVerticalOffset;
        _rigidbody.position = _positionOnMidline;
        _rigidbody.transform.position = _positionOnMidline;
    }

    private void Update()
    {
        CheckUpdateTargetLane();
        _updatedTargetLaneThisFrame = false;
    }

    private void LateUpdate()
    {
        _speedText.text = "Speed: " + (int)CurrentForwardsSpeed;
    }

    public void GarbageSlow(float playerSpeedMultiplier)
    {
        _speedMultiplier *= playerSpeedMultiplier;
        _speedMultiplier = Mathf.Max(_settings.MinForwardsSpeed / _settings.BaseForwardsSpeed, _speedMultiplier);
        _lastGarbageSlowdownTime = Time.time;
    }

    private void FixedUpdate()
    {
        if (Time.time > _lastGarbageSlowdownTime + _settings.AccelerationPauseAfterGarbageSlowdown)
        {
            if (_speedMultiplier >= 1f)
                CurrentForwardsSpeed += _settings.ForwardsAcceleration * Time.deltaTime;
            else
                CurrentForwardsSpeed += _settings.ForwardsAccelerationWhileBelowBaseSpeed * Time.deltaTime;
        }

        Vector3 priorPosition = _positionOnMidline; 

        TrackPiece trackPiece = TrackGenerator.Instance.TrackPieces[TARGET_POINT_INDEX];


        float t = trackPiece.FindTForClosestPointOnMidline(_positionOnMidline);

        Vector3 midlineVelocity = TrackMidlineVelocity(trackPiece, t);
        _positionOnMidline += midlineVelocity * Time.deltaTime;

        UpdateLanePosition();

        _rigidbody.MoveRotation(PlayerMovementProcessor.NextRotation(_settings.RotationSpeed, midlineVelocity, _rigidbody.rotation));

        trackPiece.StoreLane(0);
        Vector3 approximatedPositionOnMidline = trackPiece.BezierCurve(t);
        trackPiece.StoreLane(_lanePosition);
        Vector3 approximatedPositionAtLanePosition = trackPiece.BezierCurve(t);
        Vector3 offsetForLanePosition = approximatedPositionAtLanePosition - approximatedPositionOnMidline;

        Vector3 currentPosition = _positionOnMidline + offsetForLanePosition;

        _rigidbody.velocity = (currentPosition - _rigidbody.position) / Time.deltaTime;


        trackPiece.StoreLane(0);
        Vector3 endPosition = trackPiece.BezierCurve(1f);
        Vector3 endDirection = trackPiece.BezierCurveDerivative(1f);
        endDirection.y = 0;
        if (VectorUtils.TwoPointsAreOnDifferentSidesOfPlane(priorPosition, _positionOnMidline, endPosition, endDirection))
        {
            gameManager.AddTrackPiece();
        }
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

        // The player's y position shifts off the track near the transition between track pieces, so correct for that.
        // Only need to deal with the y position offset here because of the lane movement.
        Vector3 point = trackPiece.BezierCurve(t) + _settings.PlayerVerticalOffset * Vector3.up;
        float yDifference = point.y - _positionOnMidline.y;
        result.y += yDifference / Time.deltaTime;

        return result;
    }

    private void UpdateLanePosition()
    {
        CheckUpdateTargetLane();
        AccelerateLaneChangeSpeed(_lanePosition);

        float nextLanePosition = _lanePosition + _laneChangeSpeed / _settings.DistanceBetweenLanes * Time.deltaTime;
        if (Mathf.Sign(nextLanePosition - _currentTargetLane) != Mathf.Sign(_lanePosition - _currentTargetLane) || _lanePosition == _currentTargetLane)
        {
            // don't overshoot
            nextLanePosition = _currentTargetLane;
            _laneChangeSpeed = 0;
        }
        _lanePosition = nextLanePosition;
    }

    private void CheckUpdateTargetLane()
    {
        if (_updatedTargetLaneThisFrame)
            return;
        _updatedTargetLaneThisFrame = true;

        if (DeterministicBugReproduction.Instance.ReproduceBasedOnSaveData)
            return;

        if (LeftKey)
            _currentTargetLane--;
        if (RightKey)
            _currentTargetLane++;
        _currentTargetLane = Mathf.Clamp(_currentTargetLane, -1f, 1f);
    }

    private void AccelerateLaneChangeSpeed(float currentLane)
    {
        // This only changes _laneChangeSpeed. Adjust it more gradually than instantly moving at the maximum lane change speed,
        // to make it feel better. It still instantly stops upon reaching the target lane.

        if (DeterministicBugReproduction.Instance.OverrideTargetLane(out float overrideTargetLane))
        {
            _currentTargetLane = overrideTargetLane;
        }
        else
        {
            DeterministicBugReproduction.Instance.NextFixedUpdateTargetLane(_currentTargetLane);
        }

        float accelerationDirection = Mathf.Sign(_currentTargetLane - currentLane);

        float accelerationTime = _settings.LaneChangeSpeedupTime;
        if (Mathf.Sign(accelerationDirection) != Mathf.Sign(_laneChangeSpeed))
        {
            accelerationTime = _settings.LaneChangeTurnaroundTime;
        }

        _laneChangeSpeed += _settings.BaseLaneChangeSpeed / accelerationTime * Time.deltaTime * accelerationDirection;
        if (Mathf.Abs(_laneChangeSpeed) > _settings.BaseLaneChangeSpeed)
            _laneChangeSpeed = Mathf.Sign(_laneChangeSpeed) * _settings.BaseLaneChangeSpeed;


        float distanceFromTargetLane = Mathf.Abs(_currentTargetLane - currentLane) * _settings.DistanceBetweenLanes;
        if (distanceFromTargetLane < Mathf.Abs(_laneChangeSpeed * Time.deltaTime))
        {
            _laneChangeSpeed = Mathf.Sign(_laneChangeSpeed) * distanceFromTargetLane * Time.deltaTime;
        }
    }

    

}
