using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Diagnostics;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private float _playerSpeed = 0f;
    [SerializeField] private float _rotationSpeed = 300;
    [SerializeField] private Vector3 _playerOffset = new Vector3(0, 1.5f, 0);
    [SerializeField] private Transform[] _trackPoints;
    private bool _isInputRight = true;
    private bool _isRight = false;
    private bool _isLeft = false;
    [SerializeField] private float _sideOffset = 0;
    private float _actualSideOffset = 0;
    private Vector3 _switchTrackOffset;
    private Vector3 _lastTrackPoint;
    private int _nextTrackPoint = 1;
    

    private void Awake()
    {
        // Set the fixed delta time such that, if performance isn't an issue, the number of fixed updates is constant every frame.
        // Otherwise there's a little jitter.
        double frameRate = Screen.currentResolution.refreshRateRatio.value;
        double fixedUpdatesPerFrame = frameRate < 120 ? 2 : 1;
        Time.fixedDeltaTime = (float)(1f / (frameRate * fixedUpdatesPerFrame));

        _lastTrackPoint = _trackPoints[0].position;
    }

    private void FixedUpdate()
    {
        Vector3 displacementToNextPoint = (_trackPoints[_nextTrackPoint].position + _playerOffset) - _rigidbody.position;
        Vector3 direction = displacementToNextPoint.normalized;
        _rigidbody.velocity = direction  * _playerSpeed;

        Rotate(displacementToNextPoint);

        CheckIncrementTrackPointIndex(displacementToNextPoint);
        //Debug.Log("next index: " + _nextTrackPoint);

        if(Input.GetKeyDown(KeyCode.D)){
            SwitchTrack(_isInputRight);
        }
        if(Input.GetKeyDown(KeyCode.A)){
            SwitchTrack(!_isInputRight);
        }
    }

    private void Rotate(Vector3 displacementToNextPoint)
    {
        Vector3 displacementOnPlane = new Vector3(displacementToNextPoint.x, 0, displacementToNextPoint.z);
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

    private void CheckIncrementTrackPointIndex(Vector3 displacementToNextPoint)
    {
        Vector3 nextPosition = _rigidbody.position + _rigidbody.velocity * Time.deltaTime;
        if (Vector3.Dot(displacementToNextPoint, _trackPoints[_nextTrackPoint].position + _playerOffset - nextPosition) <= 0.0001f)
        {
            _lastTrackPoint = _trackPoints[_nextTrackPoint].position;
            _nextTrackPoint++;
            if (_nextTrackPoint == _trackPoints.Length)
            {
                _nextTrackPoint = 0; // for testing purposes (probably will add new track sections endlessly)
            }
        }
    }

    private void SwitchTrack(bool isInputRight)
    {
        if(isInputRight){
            if(!_isRight)
                _rigidbody.position = _rigidbody.position + CalculateOffsetAngle(Vector3.right);
        }

    }

    private Vector3 NextTrackPointPosition(Vector3 switchDirection){
        
        return _trackPoints[_nextTrackPoint].position + _playerOffset + 
                CalculateOffsetAngle(switchDirection);
    }

    private Vector3 CalculateOffsetAngle(Vector3 switchDirection)
    {
        return VectorUtils.RotateVectorAroundYAxis(switchDirection * _sideOffset, Vector3.SignedAngle(_trackPoints[_nextTrackPoint].position - _lastTrackPoint,
                switchDirection, Vector3 .up));
    }
}
