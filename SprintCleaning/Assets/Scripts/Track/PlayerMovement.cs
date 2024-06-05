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

    private float _laneChangeSpeed;
    private float _speedMultiplier = 1f;
    private float _lastGarbageSlowdownTime = float.NegativeInfinity;
    private int _priorSpeedTextNumber = -1;
    private TrackGenerator _gameManager;

    private Vector3 _position = Vector3.zero;
    private Vector3 _velocity = Vector3.zero;

    private float CurrentForwardsSpeed
    {
        get => _settings.BaseForwardsSpeed * _speedMultiplier;
        set => _speedMultiplier = Mathf.Clamp(value, _settings.MinForwardsSpeed, _settings.MaxForwardsSpeed) / _settings.BaseForwardsSpeed;
    }
    private float CurrentFullLaneChangeSpeed => Mathf.Min(_settings.LaneChangeSpeedCap, _settings.BaseLaneChangeSpeed * _speedMultiplier);

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
        _gameManager = TrackGenerator.Instance;
    }



    private void Start()
    {
        _rigidbody.position = TrackGenerator.Instance.TrackPieces[0].EndTransform.position + Vector3.up * _settings.PlayerVerticalOffset;
        _rigidbody.transform.position = _rigidbody.position;

        _position = _rigidbody.position;
    }

    private void LateUpdate()
    {
        // don't update the text when not necessary, to reduce garbage
        int newSpeedTextNumber = (int)CurrentForwardsSpeed;
        if (_priorSpeedTextNumber != newSpeedTextNumber)
        {
            _priorSpeedTextNumber = newSpeedTextNumber;
            _speedText.text = "Speed: " + newSpeedTextNumber;
        }
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

        TrackPiece trackPiece = TrackGenerator.Instance.TrackPieces[TARGET_POINT_INDEX];

        Vector3 currentPosition = _position - Vector3.up * _settings.PlayerVerticalOffset;

        float t = trackPiece.FindTForClosestPointOnMidline(currentPosition);
        float currentLane = trackPiece.Lane(currentPosition, t);

        trackPiece.StoreLane(currentLane);
        Vector3 trackEnd = trackPiece.EndPositionForStoredLane;

        Vector3 forwardsVelocity = ForwardsVelocity(trackPiece, t);
        bool nearEnd = (trackEnd - currentPosition).magnitude <= CurrentForwardsSpeed * Time.deltaTime;
        _velocity = forwardsVelocity;

        _rigidbody.MoveRotation(PlayerMovementProcessor.NextRotation(_settings.RotationSpeed, forwardsVelocity, _rigidbody.rotation));

        _velocity += LaneChangeVelocity(currentLane, forwardsVelocity);

        // Do the physics integration entirely separate from Unity's physics then just make the rigidbody move there. The rigidbody's only purpose
        // is to get continuous collision detection and OnTriggerEnter.
        _position += _velocity * Time.deltaTime;
        _rigidbody.velocity = (_position - _rigidbody.position) / Time.deltaTime;

        if (nearEnd)
        {
            _gameManager.AddTrackPiece();
        }
    }

    private Vector3 ForwardsVelocity(TrackPiece trackPiece, float t)
    {
        // The derivative of the bezier curve is the direction of the velocity, if timesteps were infinitely small.
        // Use the 2nd derivative to help reduce the error from discrete timesteps.
        Vector3 derivative = trackPiece.BezierCurveDerivative(t);
        Vector3 secondDerivative = trackPiece.BezierCurveSecondDerivative();
        float estimatedTChangeDuringTimestep = CurrentForwardsSpeed * Time.deltaTime / derivative.magnitude;
        Vector3 averageDerivative = derivative + estimatedTChangeDuringTimestep / 2 * secondDerivative;
        return CurrentForwardsSpeed * averageDerivative.normalized;
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

        if (!DeterministicBugReproduction.Instance.OverrideControl(out bool leftKey, out bool rightKey))
        {
            leftKey = LeftKey;
            rightKey = RightKey;
            DeterministicBugReproduction.Instance.NextFixedUpdateInputs(leftKey, rightKey);
        }

        float? targetLane = null;
        if (rightKey == leftKey) // might feel better if remember the most recent one and use that
            targetLane = null;
        else if (rightKey)
            targetLane = 1;
        else if (leftKey)
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
            if (rightKey || leftKey)
                accelerationTime = _settings.LaneChangeTurnaroundTime;
            else
                accelerationTime = _settings.LaneChangeStoppingTime;
        }

        float laneChangeSpeedSignBefore = Mathf.Sign(_laneChangeSpeed);
        _laneChangeSpeed += CurrentFullLaneChangeSpeed / accelerationTime * Time.deltaTime * accelerationDirection;
        if (Mathf.Abs(_laneChangeSpeed) > CurrentFullLaneChangeSpeed)
            _laneChangeSpeed = Mathf.Sign(_laneChangeSpeed) * CurrentFullLaneChangeSpeed;
        if (slowDown && (laneChangeSpeedSignBefore != Mathf.Sign(_laneChangeSpeed)))
            _laneChangeSpeed = 0;
    }
}
