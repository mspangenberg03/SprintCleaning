using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// could make this a monobehavior if stuff besides PlayerMovement needs it

/// <summary>
/// Represents the track and has methods to get positions along the lanes.
/// </summary>
public class TrackPositions : MonoBehaviour
{

    [SerializeField] private Transform[] _trackPoints;
    [SerializeField] private float _distanceBetweenLanes;
    [SerializeField] private float _playerVerticalOffset = 1.5f;

    public Transform[] TrackPoints => _trackPoints;

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

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;

        for (int i = 1; i < _trackPoints.Length; i++)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(LanePoint(i - 1, 0), LanePoint(i, 0));

            Gizmos.color = Color.green;
            Gizmos.DrawLine(LanePoint(i - 1, 1), LanePoint(i, 1));

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(LanePoint(i - 1, -1), LanePoint(i, -1));
        }
    }

    public Vector3 ToNextPoint(Rigidbody rigidbody, int nextPointIndex, float lane)
    {
        return LanePoint(nextPointIndex, lane) - rigidbody.position;
    }

    /// <summary>
    /// A point along the track at its midline.
    /// </summary>
    public Vector3 TrackPoint(int pointIndex)
    {
        return _trackPoints[pointIndex].position + Vector3.up * _playerVerticalOffset;
    }

    /// <summary>
    /// A point along the track at a lane.
    /// </summary>
    public Vector3 LanePoint(int pointIndex, float lane)
    {
        if (pointIndex == 0 || pointIndex == _trackPoints.Length - 1)
        {
            // At the end points of the track, there is only 1 segment of the track to consider, so just use an offset perpendicular to that lane segment.
            // Other points are on two segments so this wouldn't work for them.
            return TrackPoint(pointIndex) + LaneOffset(System.Math.Max(1, pointIndex), lane);
        }

        // Find two line segments which are offset perpendicularly to the track's midline.
        // The result of this method is the point where those two lines intersect.

        Vector3 laneOffset = LaneOffset(pointIndex, lane);
        Vector3 nextLaneOffset = LaneOffset(pointIndex + 1, lane);

        Vector3 difference = laneOffset - nextLaneOffset;
        difference.y = 0;



        if (difference.sqrMagnitude < .001f)
        {
            // Do this to deal with the cases in the stackoverflow link in LinesIntersectionPoint2D where the two lines are parallel or colinear
            return TrackPoint(pointIndex) + LaneOffset(pointIndex, lane);
        }

        Vector2 lanePointA = (_trackPoints[pointIndex - 1].position + laneOffset).To2D();
        Vector2 lanePointB = (_trackPoints[pointIndex].position + laneOffset).To2D();

        Vector2 nextLanePointA = (_trackPoints[pointIndex].position + nextLaneOffset).To2D();
        Vector2 nextLanePointB = (_trackPoints[pointIndex + 1].position + nextLaneOffset).To2D();

        Vector3 result = VectorUtils.LinesIntersectionPoint2D(lanePointA, lanePointB, nextLanePointA, nextLanePointB).To3D();
        result.y = TrackPoint(pointIndex).y;

        return result;
    }

    public Vector2 ClosestPointOnTrack(int endpointIndex, Vector3 point)
    {
        Vector2 start = TrackPoint(endpointIndex - 1).To2D();
        Vector2 end = TrackPoint(endpointIndex).To2D();
        return VectorUtils.ClosestPointOnLineOrSegment2D(point.To2D(), start, end, false);
    }

    /// <summary>
    /// Computes a vector perpendicular to a segment of the track which is the offset from the middle lane to a given lane.
    /// </summary>
    /// <param name="laneSegmentEndpointIndex">The higher index of two indexes which define a segment of the path (e.g. 4 for indexes 3 and 4).</param>
    /// <param name="lane">The lane, from -1 to 1.</param>
    public Vector3 LaneOffset(int laneSegmentEndpointIndex, float lane)
    {
        // Determine the vector from the middle lane to the a lane.
        Vector3 pathDirection = _trackPoints[laneSegmentEndpointIndex].position - _trackPoints[laneSegmentEndpointIndex - 1].position;
        float trackAngle = Vector3.SignedAngle(pathDirection, Vector3.forward, Vector3.up);

        Vector3 laneOffset = lane * _distanceBetweenLanes * Vector3.right; // the offset if the track isn't rotated
        return VectorUtils.RotateVectorAroundYAxis(laneOffset, trackAngle);
    }

    public float ConvertPositionToLane(int endpointIndex, Vector3 position)
    {
        Vector2 closestPoint = ClosestPointOnTrack(endpointIndex, position);
        float distance = (closestPoint - position.To2D()).magnitude;

        bool toLeft = VectorUtils.PointIsToLeftOfVector(TrackPoint(endpointIndex - 1), TrackPoint(endpointIndex), position - Vector3.up * _playerVerticalOffset);
        float sign = toLeft ? -1f : 1f;
        return distance / _distanceBetweenLanes * sign;
    }

    public Vector3 PositionToBeOnLane(int endpointIndex, float lane, Vector3 position)
    {
        Vector2 targetLaneStart = LanePoint(endpointIndex - 1, lane).To2D();
        Vector2 targetLaneEnd = LanePoint(endpointIndex, lane).To2D();

        Vector3 result = VectorUtils.ClosestPointOnSegment2D(position.To2D(), targetLaneStart, targetLaneEnd).To3D();
        result.y = position.y;
        return result;
    }
}
