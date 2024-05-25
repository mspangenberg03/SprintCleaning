using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Diagnostics;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private float _playerSpeed = 0f;
    [SerializeField] private float _rotationSpeed = 300;
    [SerializeField] private Vector3 _playerOffset = new Vector3(0, 1.5f, 0);
    [SerializeField] private float _distanceBetweenLanes = 1.5f;
    [SerializeField] private Transform[] _trackPoints;
    //private bool _isInputRight = true;
    //private bool _isRight = false;
    //private bool _isLeft = false;
    //private float _actualSideOffset = 0;
    //private Vector3 _switchTrackOffset;
    private int _nextPointIndex = 0;
    private int _currentLane;
    //private Vector3 _currentLaneOffset; // relative to the center lane

    private bool _fixedUpdateHappened;
    private bool _leftKeyDownDuringFrameWithoutFixedUpdate;
    private bool _rightKeyDownDuringFrameWithoutFixedUpdate;
    //private List<(Vector3, Vector3)> _gizmoWhiteLines = new();
    //private List<(Vector3, Vector3)> _gizmoGreenLines = new();

    private Vector3 ToTarget => TargetPosition() - _rigidbody.position;
    private bool LeftKeyDown => Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow);
    private bool RightKeyDown => Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow);




    private void Awake()
    {
        StartOnTrack();

        // Set the fixed delta time such that, if performance isn't an issue, the number of fixed updates is constant every frame.
        // Otherwise there's a little jitter.
        double frameRate = Screen.currentResolution.refreshRateRatio.value;
        double fixedUpdatesPerFrame = frameRate < 120 ? 2 : 1;
        Time.fixedDeltaTime = (float)(1f / (frameRate * fixedUpdatesPerFrame));
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;

        for (int i = 1; i < _trackPoints.Length; i++)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(TargetPosition(i - 1, 0), TargetPosition(i, 0));

            Gizmos.color = Color.green;
            Gizmos.DrawLine(TargetPosition(i - 1, 1), TargetPosition(i, 1));

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(TargetPosition(i - 1, -1), TargetPosition(i, -1));
        }

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(TargetPosition(), .5f);
    }

    private void StartOnTrack()
    {
        _currentLane = 0;
        _rigidbody.position = _trackPoints[0].position + _playerOffset;
        _rigidbody.transform.position = _rigidbody.position;
        _nextPointIndex = 1;
    }

    private void FixedUpdate()
    {
        Vector3 direction = ToTarget.normalized;
        _rigidbody.velocity = direction * _playerSpeed;

        Rotate();

        CheckIncrementTrackPointIndex();

        if (RightKeyDown || _rightKeyDownDuringFrameWithoutFixedUpdate)
        {
            SwitchTrack(1);
        }
        if (LeftKeyDown || _leftKeyDownDuringFrameWithoutFixedUpdate)
        {
            SwitchTrack(-1);
        }

        _rightKeyDownDuringFrameWithoutFixedUpdate = false;
        _leftKeyDownDuringFrameWithoutFixedUpdate = false;
        _fixedUpdateHappened = true;
    }

    private void Update()
    {
        if (!_fixedUpdateHappened)
        {
            if (RightKeyDown)
                _rightKeyDownDuringFrameWithoutFixedUpdate = true;
            if (LeftKeyDown)
                _leftKeyDownDuringFrameWithoutFixedUpdate = true;
        }

        _fixedUpdateHappened = false;
    }

    private void Rotate()
    {
        Vector3 displacementOnPlane = new Vector3(ToTarget.x, 0, ToTarget.z);
        float movementAngle = Quaternion.FromToRotation(Vector3.forward, displacementOnPlane).eulerAngles.y;
        Vector3 currentAngles = _rigidbody.rotation.eulerAngles;
        float currentYAngle = _rigidbody.rotation.eulerAngles.y;
        float angleChange = movementAngle - currentYAngle;
        angleChange = ((angleChange + 540) % 360) - 180; // make change be from -180 to 180
        float maxChange = _rotationSpeed * Time.deltaTime;
        if (Mathf.Abs(angleChange) > maxChange)
        {
            angleChange = Mathf.Sign(angleChange) * maxChange;
        }

        float nextAngle = currentYAngle + angleChange;

        _rigidbody.MoveRotation(Quaternion.Euler(currentAngles.x, nextAngle, currentAngles.z));
    }

    private void CheckIncrementTrackPointIndex()
    {
        // determine what the player's position will be after the physics engine performs its internal physics update (which happens after fixed update)
        Vector3 newPosition = _rigidbody.position + _rigidbody.velocity * Time.deltaTime;

        // If the direction to the target will become opposite, then it's about to get past the next point
        if (Vector3.Dot(ToTarget, TargetPosition() - newPosition) <= 0.0001f)
        {
            _nextPointIndex++;
            if (_nextPointIndex == _trackPoints.Length)
            {
                StartOnTrack(); // for testing purposes (probably will add new track sections endlessly)
            }
        }
    }

    private void SwitchTrack(int laneChange)
    {
        Vector3 priorLaneOffset = CalculateLaneOffset();

        int nextLane = System.Math.Clamp(_currentLane + laneChange, -1, 1);
        if (nextLane == _currentLane)
            return;
        _currentLane = nextLane;

        Vector3 newLaneOffset = CalculateLaneOffset();

        Vector3 shift = newLaneOffset - priorLaneOffset;
        _rigidbody.position += shift;
    }

    private Vector3 CalculateLaneOffset() => CalculateLaneOffset(_nextPointIndex, _currentLane);

    private Vector3 CalculateLaneOffset(int endPointIndex, int lane)
    {
        // Determine the vector from the middle lane to the current lane.
        Vector3 pathDirection = _trackPoints[endPointIndex].position - _trackPoints[endPointIndex - 1].position;
        float trackAngle = Vector3.SignedAngle(pathDirection, Vector3.forward, Vector3.up);

        Vector3 laneOffset = lane * _distanceBetweenLanes * Vector3.right; // the offset if the track isn't rotated
        return VectorUtils.RotateVectorAroundYAxis(laneOffset, trackAngle);
    }

    private Vector3 TargetPosition() => TargetPosition(_nextPointIndex, _currentLane);
    private Vector3 TargetPosition(int endPointIndex, int lane)
    {
        if (endPointIndex == 0 || endPointIndex == _trackPoints.Length - 1)
        {
            return _trackPoints[endPointIndex].position + CalculateLaneOffset(System.Math.Max(1, endPointIndex), lane) + _playerOffset;
        }

        Vector3 laneOffset = CalculateLaneOffset(endPointIndex, lane);
        Vector3 nextLaneOffset = CalculateLaneOffset(endPointIndex + 1, lane);

        Vector3 difference = laneOffset - nextLaneOffset;
        difference.y = 0;

        if (difference.sqrMagnitude < .001f)
        {
            // Do this to deal with the cases in the stackoverflow link where the two lines are parallel or colinear
            return _trackPoints[endPointIndex].position + CalculateLaneOffset(endPointIndex, lane) + _playerOffset;
        }

        Vector3 lanePoint1 = _trackPoints[endPointIndex - 1].position + laneOffset;
        Vector3 lanePoint2 = _trackPoints[endPointIndex].position + laneOffset;

        Vector3 nextLanePoint1 = _trackPoints[endPointIndex].position + nextLaneOffset;
        Vector3 nextLanePoint2 = _trackPoints[endPointIndex + 1].position + nextLaneOffset;

        // Find where those two lines intersect (on the x-z plane)
        // https://stackoverflow.com/questions/563198/how-do-you-detect-where-two-line-segments-intersect 
        // except allow t to be any value
        Vector3 p = lanePoint1;
        Vector3 r = lanePoint2 - p;
        Vector3 q = nextLanePoint1;
        Vector3 s = nextLanePoint2 - nextLanePoint1;

        float t = Cross2D(q - p, s) / Cross2D(r, s); // t = (q − p) x s / (r x s)
        return p + t * r + _playerOffset;


        // two dimensional cross product like in the link
        float Cross2D(Vector3 v, Vector3 w)
        {
            return v.x * w.z - v.z * w.x;
        }
    }


}
