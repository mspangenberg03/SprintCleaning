using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private float _playerSpeed = 0f;
    [SerializeField] private float _rotationSpeed = 300;
    [SerializeField] private Vector3 _playerOffset = new Vector3(0, 1.5f, 0);
    [SerializeField] private float _distanceBetweenLanes = 1.5f;
    [SerializeField] private Transform[] _trackPoints;

    private PlayerMovementLanes _lanes;


    private int _nextPointIndex = 0;
    private int _currentLane;

    private bool _fixedUpdateHappenedThisFrame;
    private bool _leftKeyDownDuringFrameWithoutFixedUpdate;
    private bool _rightKeyDownDuringFrameWithoutFixedUpdate;

    private bool LeftKeyDown => Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow);
    private bool RightKeyDown => Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow);




    private void Awake()
    {
        _lanes = new PlayerMovementLanes(_trackPoints, _distanceBetweenLanes, _playerOffset);
        StartOnTrack();
        PlayerMovementProcessor.SetFixedDeltaTime();
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;

        for (int i = 1; i < _trackPoints.Length; i++)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(_lanes.Point(i - 1, 0), _lanes.Point(i, 0));

            Gizmos.color = Color.green;
            Gizmos.DrawLine(_lanes.Point(i - 1, 1), _lanes.Point(i, 1));

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(_lanes.Point(i - 1, -1), _lanes.Point(i, -1));
        }

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(TargetPoint(), .5f);
    }

    private void StartOnTrack()
    {
        _currentLane = 0;
        StopAllCoroutines();
        _rigidbody.position = _trackPoints[0].position + _playerOffset;
        _rigidbody.transform.position = _rigidbody.position;
        _nextPointIndex = 1;
    }

    private void FixedUpdate()
    {
        Vector3 directionToNextPoint = (TargetPoint() - _rigidbody.position).normalized;
        _rigidbody.velocity = directionToNextPoint * _playerSpeed;

        _rigidbody.MoveRotation(PlayerMovementProcessor.NextRotation(_rotationSpeed, directionToNextPoint, _rigidbody.rotation));

        CheckIncrementTrackPointIndex();

        // Don't check inputs if fixed update already ran this frame because it already checked inputs.
        if (!_fixedUpdateHappenedThisFrame)
        {
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
        }
        
        _fixedUpdateHappenedThisFrame = true;
    }

    private void Update()
    {
        // If FixedUpdate didn't run yet during this frame, cache key presses and will apply the actions next
        // time FixedUpdate runs.
        if (!_fixedUpdateHappenedThisFrame)
        {
            if (RightKeyDown)
                _rightKeyDownDuringFrameWithoutFixedUpdate = true;
            if (LeftKeyDown)
                _leftKeyDownDuringFrameWithoutFixedUpdate = true;
        }

        _fixedUpdateHappenedThisFrame = false;
    }


    private void CheckIncrementTrackPointIndex()
    {
        // determine what the player's position will be after the physics engine performs its internal physics update (which happens after fixed update)
        Vector3 newPosition = _rigidbody.position + _rigidbody.velocity * Time.deltaTime;
        Vector3 targetPoint = TargetPoint();

        // If the direction to the target will become opposite, then it's about to get past the next point
        if (Vector3.Dot(targetPoint - _rigidbody.position, targetPoint - newPosition) <= 0.0001f)
        {
            _nextPointIndex++;
            if (_nextPointIndex == _trackPoints.Length)
            {
                StartOnTrack(); // for testing purposes (probably will add new track sections endlessly)
            }
        }
    }

    private Vector3 TargetPoint()
    {
        return _lanes.Point(_nextPointIndex, _currentLane);
    }

    private void SwitchTrack(int laneChange)
    {
        Vector3 priorLaneOffset = _lanes.LaneOffset(_nextPointIndex, _currentLane);

        int nextLane = System.Math.Clamp(_currentLane + laneChange, -1, 1);
        if (nextLane == _currentLane)
            return;
        _currentLane = nextLane;

        Vector3 newLaneOffset = _lanes.LaneOffset(_nextPointIndex, _currentLane);

        Vector3 shift = newLaneOffset - priorLaneOffset;
        _rigidbody.position += shift;
    }
}
