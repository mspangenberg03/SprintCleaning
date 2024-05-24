using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private float _playerSpeed = 0f;
    [SerializeField] private float _rotationSpeed = 300;
    [SerializeField] private Transform[] _trackPoints;
    private int _nextTrackPoint = 1;

    private void Awake()
    {
        // Set the fixed delta time such that, if performance isn't an issue, the number of fixed updates is constant every frame.
        // Otherwise there's a little jitter.
        double frameRate = Screen.currentResolution.refreshRateRatio.value;
        double fixedUpdatesPerFrame = frameRate < 120 ? 2 : 1;
        Time.fixedDeltaTime = (float)(1f / (frameRate * fixedUpdatesPerFrame));
    }

    private void FixedUpdate()
    {
        Vector3 displacementToNextPoint = _trackPoints[_nextTrackPoint].position - _rigidbody.position;
        Vector3 direction = displacementToNextPoint.normalized;
        _rigidbody.velocity = direction * _playerSpeed;

        Rotate(displacementToNextPoint);

        CheckIncrementTrackPointIndex(displacementToNextPoint);
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
        if (Vector3.Dot(displacementToNextPoint, _trackPoints[_nextTrackPoint].position - nextPosition) < 0)
        {
            _nextTrackPoint++;
            if (_nextTrackPoint == _trackPoints.Length)
            {
                _nextTrackPoint = 0; // for testing purposes (probably will add new track sections endlessly)
            }
        }
    }
}
