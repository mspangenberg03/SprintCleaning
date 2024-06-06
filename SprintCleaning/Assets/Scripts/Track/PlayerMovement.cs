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


    private Vector3 _position;
    private float _laneChangeSpeed;
    private float _speedMultiplier = 1f;
    private float _lastGarbageSlowdownTime = float.NegativeInfinity;
    private float _currentTargetLane;
    private TrackGenerator gameManager;

    //private Vector3 _prior

    private float CurrentForwardsSpeed
    {
        get => _settings.BaseForwardsSpeed * _speedMultiplier;
        set => _speedMultiplier = Mathf.Clamp(value, _settings.MinForwardsSpeed, _settings.MaxForwardsSpeed) / _settings.BaseForwardsSpeed;
    }
    //private float CurrentFullLaneChangeSpeed => _settings.BaseLaneChangeSpeed;// Mathf.Min(_settings.LaneChangeSpeedCap, _settings.BaseLaneChangeSpeed * _speedMultiplier);

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
        _position = TrackGenerator.Instance.TrackPieces[0].EndTransform.position + Vector3.up * _settings.PlayerVerticalOffset;
        _rigidbody.position = _position;
        _rigidbody.transform.position = _position;
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

        Vector3 priorPosition = _position; 

        TrackPiece trackPiece = TrackGenerator.Instance.TrackPieces[TARGET_POINT_INDEX];


        float t = trackPiece.FindTForClosestPointOnMidline(_position);

        float currentLane = trackPiece.Lane(_position, t);

        trackPiece.StoreLane(currentLane);

        Vector3 mainVelocity = MainVelocity(trackPiece, t);
        Vector3 laneChangeVelocity = LaneChangeVelocity(currentLane, mainVelocity);
        Vector3 velocity = mainVelocity + laneChangeVelocity;
        _position += velocity * Time.deltaTime;

        _rigidbody.MoveRotation(PlayerMovementProcessor.NextRotation(_settings.RotationSpeed, mainVelocity, _rigidbody.rotation));


        _rigidbody.velocity = (_position - _rigidbody.position) / Time.deltaTime;


        trackPiece.StoreLane(0);
        Vector3 endPosition = trackPiece.BezierCurve(1f);
        Vector3 endDirection = trackPiece.BezierCurveDerivative(1f);
        endDirection.y = 0;
        if (VectorUtils.TwoPointsAreOnDifferentSidesOfPlane(priorPosition, _position, endPosition, endDirection))
        {
            gameManager.AddTrackPiece();
        }
    }

    private Vector3 MainVelocity(TrackPiece trackPiece, float t)
    {
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
        float yDifference = point.y - _position.y;
        result.y += yDifference / Time.deltaTime;

        return result;
    }
    

    private Vector3 LaneChangeVelocity(float currentLane, Vector3 forwardsVelocity)
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

        bool leftKey, rightKey;
        if (!DeterministicBugReproduction.Instance.OverrideControl(out leftKey, out rightKey))
        {
            leftKey = LeftKey;
            rightKey = RightKey;
            DeterministicBugReproduction.Instance.NextFixedUpdateInputs(leftKey, rightKey);
        }

        if (leftKey)
            _currentTargetLane--;
        if (rightKey)
            _currentTargetLane++;
        _currentTargetLane = Mathf.Clamp(_currentTargetLane, -1f, 1f);

        //float? targetLane = null;
        //if (rightKey == leftKey) // might feel better if remember the most recent one and use that
        //    targetLane = null;
        //else if (rightKey)
        //    targetLane = 1;
        //else if (leftKey)
        //    targetLane = -1;

        //bool slowDown = currentLane == targetLane || !targetLane.HasValue;

        float accelerationDirection = Mathf.Sign(_currentTargetLane - currentLane);

        float accelerationTime = _settings.LaneChangeSpeedupTime;
        if (Mathf.Sign(accelerationDirection) != Mathf.Sign(_laneChangeSpeed))
        {
            if (rightKey || leftKey)
                accelerationTime = _settings.LaneChangeTurnaroundTime;
            else
                accelerationTime = _settings.LaneChangeStoppingTime;
        }

        //float laneChangeSpeedSignBefore = Mathf.Sign(_laneChangeSpeed);
        _laneChangeSpeed += _settings.BaseLaneChangeSpeed / accelerationTime * Time.deltaTime * accelerationDirection;
        if (Mathf.Abs(_laneChangeSpeed) > _settings.BaseLaneChangeSpeed)
            _laneChangeSpeed = Mathf.Sign(_laneChangeSpeed) * _settings.BaseLaneChangeSpeed;


        float distanceFromTargetLane = Mathf.Abs(_currentTargetLane - currentLane) * _settings.DistanceBetweenLanes;
        if (distanceFromTargetLane < Mathf.Abs(_laneChangeSpeed * Time.deltaTime))
        {
            _laneChangeSpeed = Mathf.Sign(_laneChangeSpeed) * distanceFromTargetLane * Time.deltaTime;
        }


        //if (slowDown && (laneChangeSpeedSignBefore != Mathf.Sign(_laneChangeSpeed)))
        //    _laneChangeSpeed = 0;
    }
}
