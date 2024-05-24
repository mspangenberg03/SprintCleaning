using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private float _playerSpeed = 0f;
    [SerializeField] private Transform[] _trackPoints;
    private Vector3 _movement = new Vector3(0, 0, 0);
    private int _nextTrackPoint = 1;

    private void FixedUpdate()
    {
        Vector3 displacement = _trackPoints[_nextTrackPoint].position - _rigidbody.position;
        Vector3 direction = displacement.normalized;
        _rigidbody.velocity = direction * _playerSpeed;

        Vector3 nextPosition = _rigidbody.position + _rigidbody.velocity * Time.deltaTime;
        if(Vector3.Dot(displacement, _trackPoints[_nextTrackPoint].position - nextPosition) < 0){
            _nextTrackPoint ++;
        }
    }
}
