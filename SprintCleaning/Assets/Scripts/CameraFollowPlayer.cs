using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    [SerializeField] private Rigidbody _player;
    [SerializeField] private Vector3 _offset = Vector3.zero;
    [SerializeField] private float _xRotation;

    private void LateUpdate()
    {
        Vector3 rotation = _player.transform.rotation.eulerAngles;
        rotation.x = _xRotation;
        rotation.z = 0;
        transform.rotation = Quaternion.Euler(rotation);
        transform.position = _player.transform.position + RotateVectorAroundYAxis(_offset, -_player.rotation.eulerAngles.y);
    }

    private Vector3 RotateVectorAroundYAxis(Vector3 direction, float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);
        float x = cos * direction.x - sin * direction.z;
        float z = sin * direction.x + cos * direction.z;
        return new Vector3(x, direction.y, z);
    }
}
