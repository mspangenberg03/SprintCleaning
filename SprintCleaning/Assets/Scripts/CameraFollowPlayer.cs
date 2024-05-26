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
        transform.position = _player.transform.position + VectorUtils.RotateVectorAroundYAxis(_offset, -_player.rotation.eulerAngles.y);
    }
}
