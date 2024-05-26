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

    private TrackPositions _track;


    private int _nextPointIndex = 0;
    private float _targetLane;

    private bool _fixedUpdateHappenedThisFrame;
    private bool _leftKeyDownDuringFrameWithoutFixedUpdate;
    private bool _rightKeyDownDuringFrameWithoutFixedUpdate;

    private bool LeftKeyDown => Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow);
    private bool RightKeyDown => Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow);




    private void Awake()
    {
        _track = new TrackPositions(_trackPoints, _distanceBetweenLanes, _playerOffset);
        StartOnTrack();
        PlayerMovementProcessor.SetFixedDeltaTime();
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
            _track.DrawGizmos(_nextPointIndex, CurrentLane());
    }

    private void StartOnTrack()
    {
        _targetLane = 0;
        StopAllCoroutines();
        _rigidbody.position = _track.TrackPoint(0) + _playerOffset;
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
                SwitchTrack(1f);
            }
            if (LeftKeyDown || _leftKeyDownDuringFrameWithoutFixedUpdate)
            {
                SwitchTrack(-1f);
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
        return _track.LanePoint(_nextPointIndex, CurrentLane());
    }

    private void SwitchTrack(float laneChange)
    {
        float nextLane = Mathf.Clamp(_targetLane + laneChange, -1, 1);
        if (nextLane == _targetLane)
            return;
        _targetLane = nextLane;

        //Vector2 targetLaneStart = _track.LanePoint(_nextPointIndex - 1, _targetLane).To2D();
        //Vector2 targetLaneEnd = _track.LanePoint(_nextPointIndex, _targetLane).To2D();
        //Vector2 playerPoint = (_rigidbody.position - _playerOffset).To2D();

        //Vector3 newPosition = VectorUtils.ClosestPointOnLineSegment2D(playerPoint, targetLaneStart, targetLaneEnd).To3D();
        //newPosition.y = _rigidbody.position.y;

        StopAllCoroutines();
        StartCoroutine(SwitchLane());

        //_rigidbody.position = PositionToBeOnTargetLane();
    }

    private float CurrentLane()
    {
        Vector2 closestPoint = _track.ClosestPointOnTrack(_nextPointIndex, _rigidbody.position);
        float distance = (closestPoint - _rigidbody.position.To2D()).magnitude;

        bool toLeft = VectorUtils.PointIsToLeftOfVector(_track.TrackPoint(_nextPointIndex - 1), _track.TrackPoint(_nextPointIndex), _rigidbody.position - _playerOffset);
        float sign = toLeft ? -1f : 1f;
        return distance / _distanceBetweenLanes * sign;
    }

    

    private IEnumerator SwitchLane()
    {
        while (true)
        {
            Vector3 targetLanePoint = PositionToBeOnTargetLane();

            Vector3 toTargetLanePoint = targetLanePoint - _rigidbody.position;

            if (toTargetLanePoint.sqrMagnitude < .001f)
            {
                break;
            }

            _rigidbody.position += Vector3.MoveTowards(Vector3.zero, toTargetLanePoint, 1.5f * Time.deltaTime);

            yield return new WaitForFixedUpdate();
        }
    }

    private Vector3 PositionToBeOnTargetLane()
    {
        Vector2 targetLaneStart = _track.LanePoint(_nextPointIndex - 1, _targetLane).To2D();
        Vector2 targetLaneEnd = _track.LanePoint(_nextPointIndex, _targetLane).To2D();
        Vector2 playerPoint = (_rigidbody.position - _playerOffset).To2D();

        Vector3 result = VectorUtils.ClosestPointOnSegment2D(playerPoint, targetLaneStart, targetLaneEnd).To3D();
        result.y = _rigidbody.position.y;
        return result;
    }
}
