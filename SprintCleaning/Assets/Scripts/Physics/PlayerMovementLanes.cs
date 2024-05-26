using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementLanes
{

    private Transform[] _trackPoints;
    private float _distanceBetweenLanes;
    private Vector3 _playerOffset;

    public PlayerMovementLanes(Transform[] trackPoints, float distanceBetweenLanes, Vector3 playerOffset)
    {
        _trackPoints = trackPoints;
        _distanceBetweenLanes = distanceBetweenLanes;
        _playerOffset = playerOffset;
    }

    public Vector3 ToNextPoint(Rigidbody rigidbody, int nextPointIndex, int currentLane)
    {
        return Point(nextPointIndex, currentLane) - rigidbody.position;
    }

    /// <summary>
    /// A point along the track at a lane.
    /// </summary>
    public Vector3 Point(int pointIndex, int lane)
    {
        if (pointIndex == 0 || pointIndex == _trackPoints.Length - 1)
        {
            // At the end points of the track, there is only 1 segment of the track to consider, so just use an offset perpendicular to that lane segment.
            // Other points are on two segments so this wouldn't work for them.
            return _trackPoints[pointIndex].position + LaneOffset(System.Math.Max(1, pointIndex), lane) + _playerOffset;
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
            return _trackPoints[pointIndex].position + LaneOffset(pointIndex, lane) + _playerOffset;
        }

        Vector2 lanePointA = (_trackPoints[pointIndex - 1].position + laneOffset).To2D();
        Vector2 lanePointB = (_trackPoints[pointIndex].position + laneOffset).To2D();

        Vector2 nextLanePointA = (_trackPoints[pointIndex].position + nextLaneOffset).To2D();
        Vector2 nextLanePointB = (_trackPoints[pointIndex + 1].position + nextLaneOffset).To2D();

        Vector3 result = VectorUtils.LinesIntersectionPoint2D(lanePointA, lanePointB, nextLanePointA, nextLanePointB).To3D();
        result.y = _trackPoints[pointIndex].position.y;
        result += _playerOffset;
        return result;
    }

    /// <summary>
    /// Computes a vector perpendicular to a segment of the track which is the offset from the middle lane to a given lane.
    /// </summary>
    /// <param name="laneSegmentEndpointIndex">The higher index of two indexes which define a segment of the path (e.g. 4 for indexes 3 and 4).</param>
    /// <param name="lane">The lane, from -1 to 1.</param>
    public Vector3 LaneOffset(int laneSegmentEndpointIndex, int lane)
    {
        // Determine the vector from the middle lane to the a lane.
        Vector3 pathDirection = _trackPoints[laneSegmentEndpointIndex].position - _trackPoints[laneSegmentEndpointIndex - 1].position;
        float trackAngle = Vector3.SignedAngle(pathDirection, Vector3.forward, Vector3.up);

        Vector3 laneOffset = lane * _distanceBetweenLanes * Vector3.right; // the offset if the track isn't rotated
        return VectorUtils.RotateVectorAroundYAxis(laneOffset, trackAngle);
    }
}
