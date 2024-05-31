using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// could make this a monobehavior if stuff besides PlayerMovement needs it

/// <summary>
/// Represents the track and has methods to get positions along the lanes.
/// </summary>
public class TrackPositions : MonoBehaviour
{

    [SerializeField] private float _distanceBetweenLanes = 1.5f;
    [SerializeField] private float _playerVerticalOffset = 1.5f;
    public float PlayerVerticalOffset => _playerVerticalOffset;
    public float DistanceBetweenLanes => _distanceBetweenLanes;


    private static TrackPositions _instance;
    public static TrackPositions Instance 
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<TrackPositions>();
            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
    }

    public Vector3 TrackPoint(int pointIndex)
    {
        return TrackGenerator.Instance.TrackPieces[pointIndex].EndTransform.position + Vector3.up * _playerVerticalOffset;
    }
}
