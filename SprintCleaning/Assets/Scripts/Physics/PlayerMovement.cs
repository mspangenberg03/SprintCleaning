using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private float _playerSpeed = 0f;
    [SerializeField] private float _laneChangeSpeed = 30f;
    [SerializeField] private float _rotationSpeed = 300;
    [SerializeField] private bool _discreteMovement = false;
    private TrackPositions _track;


    private GameManager gameManager;



    public int _nextPointIndex = 0;
    public int _lastPointIndex = -1;
    private float _targetLane;

    private bool _fixedUpdateHappenedThisFrame;
    private bool _leftKeyDownDuringFrameWithoutFixedUpdate;
    private bool _rightKeyDownDuringFrameWithoutFixedUpdate;

    private bool LeftKeyDown => Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow);
    private bool RightKeyDown => Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow);
    private bool LeftKey => Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
    private bool RightKey => Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);


    private void Awake() {
        gameManager = GameManager.Instance;
        _track = TrackPositions.Instance;
    }

    private void Start()
    {
        StartOnTrack();
        PlayerMovementProcessor.SetFixedDeltaTime();
    }

    private void StartOnTrack()
    {
        _targetLane = 0;
        _rigidbody.position = _track.TrackPoint(0);
        _rigidbody.transform.position = _rigidbody.position;
        _nextPointIndex = 1;
    }

    private void FixedUpdate()
    {
        Vector3 directionToNextPoint = (TargetPoint() - _rigidbody.position).normalized;
        _rigidbody.velocity = directionToNextPoint * _playerSpeed;

        if (!_discreteMovement)
        {
            if (RightKey)
                SwitchTrack(1f);
            if (LeftKey)
                SwitchTrack(-1f);
            if (!RightKey && !LeftKey)
                _targetLane = _track.ConvertPositionToLane(_nextPointIndex, _rigidbody.position);
        }

        if (!_discreteMovement || LeftKey || RightKey)
        {
            IncludeVelocityTowardsTargetLane();
        }
        _rigidbody.MoveRotation(PlayerMovementProcessor.NextRotation(_rotationSpeed, directionToNextPoint, _rigidbody.rotation));

        CheckIncrementTrackPointIndex();

        if (_discreteMovement)
        {
            // Don't check inputs if fixed update already ran this frame because it already checked inputs.
            if (!_fixedUpdateHappenedThisFrame)
            {
                if (RightKeyDown || _rightKeyDownDuringFrameWithoutFixedUpdate)
                    SwitchTrack(1f);
                if (LeftKeyDown || _leftKeyDownDuringFrameWithoutFixedUpdate)
                    SwitchTrack(-1f);
                _rightKeyDownDuringFrameWithoutFixedUpdate = false;
                _leftKeyDownDuringFrameWithoutFixedUpdate = false;
            }
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
            if (_nextPointIndex == _track.TrackPoints.Length)
            {
                _nextPointIndex = 1; // for testing purposes (probably will add new track sections endlessly)
            }
        }
    }

    private Vector3 TargetPoint()
    {
        return _track.LanePoint(_nextPointIndex, _track.ConvertPositionToLane(_nextPointIndex, _rigidbody.position));
    }

    private void SwitchTrack(float laneChange)
    {
        float nextLane = Mathf.Clamp(_targetLane + laneChange, -1, 1);
        if (nextLane != _targetLane)
            _targetLane = nextLane;
    }

    

    private void IncludeVelocityTowardsTargetLane()
    {
        float currentLane = _track.ConvertPositionToLane(_nextPointIndex, _rigidbody.position);
        if (currentLane == _targetLane)
            return;


        Vector3 targetLanePoint = _track.PositionToBeOnLane(_nextPointIndex, _targetLane, _rigidbody.position);
        Vector3 toTargetLanePoint = targetLanePoint - _rigidbody.position;

        Vector3 laneChangeVelocity = _laneChangeSpeed * toTargetLanePoint.normalized;
        Vector3 nextPosition = _rigidbody.position + laneChangeVelocity * Time.deltaTime;

        if (Vector3.Dot(toTargetLanePoint, targetLanePoint - nextPosition) <= 0.0001f)
        {
            // don't overshoot
            laneChangeVelocity = toTargetLanePoint / Time.deltaTime;
        }

        _rigidbody.velocity += laneChangeVelocity;
    }

    //private Vector3 PositionToBeOnTargetLane()
    //{
    //    Vector2 targetLaneStart = _track.LanePoint(_nextPointIndex - 1, _targetLane).To2D();
    //    Vector2 targetLaneEnd = _track.LanePoint(_nextPointIndex, _targetLane).To2D();
    //    Vector2 playerPoint = (_rigidbody.position - _playerOffset).To2D();

    //    Vector3 result = VectorUtils.ClosestPointOnSegment2D(playerPoint, targetLaneStart, targetLaneEnd).To3D();
    //    result.y = _rigidbody.position.y;
    //    return result;
    //}

    private void OnTriggerEnter(Collider other) {
        if(other.CompareTag("trackCheckpoint")){
            if(_lastPointIndex > -1)
                gameManager.InstantiateTrackSegment();
                _lastPointIndex ++;
                if(_lastPointIndex == _track.TrackPoints.Length){
                    _lastPointIndex = 0;
                }
        }
    }
}
