using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// could make this a monobehavior if stuff besides PlayerMovement needs it

/// <summary>
/// Represents the track and has methods to get positions along the lanes.
/// </summary>
public class TrackPositions
{

    private Transform[] _trackPoints;
    private float _distanceBetweenLanes;
    private Vector3 _playerOffset;

    public int NumPoints => _trackPoints.Length;

    public TrackPositions(Transform[] trackPoints, float distanceBetweenLanes, Vector3 playerOffset)
    {
        _trackPoints = trackPoints;
        _distanceBetweenLanes = distanceBetweenLanes;
        _playerOffset = playerOffset;
    }

    public void DrawGizmos(int nextPointIndex, float lane)
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(LanePoint(1, -1, true), .5f);
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(LanePoint(1, 0), .5f);
        Gizmos.color = Color.black;
        Gizmos.DrawSphere(LanePoint(1, 1), .5f);


        //for (int i = 1; i < _trackPoints.Length; i++)
        //{

        //    Gizmos.color = Color.white;
        //    Debug.Log(i + "left lane point 1: " + LanePoint(i - 1, -1));
        //    Gizmos.DrawLine(LanePoint(i - 1, 0), LanePoint(i, 0));

        //    Gizmos.color = Color.green;
        //    Gizmos.DrawLine(LanePoint(i - 1, 1), LanePoint(i, 1));

        //    Gizmos.color = Color.blue;
        //    Gizmos.DrawLine(LanePoint(i - 1, -1), LanePoint(i, -1));
        //}

        //Gizmos.color = Color.red;
        //Gizmos.DrawSphere(LanePoint(nextPointIndex, lane), .5f);
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
        return _trackPoints[pointIndex].position;
    }

    /// <summary>
    /// A point along the track at a lane.
    /// </summary>
    public Vector3 LanePoint(int pointIndex, float lane, bool test = false)
    {
        if (pointIndex == 0 || pointIndex == _trackPoints.Length - 1)
        {
            // At the end points of the track, there is only 1 segment of the track to consider, so just use an offset perpendicular to that lane segment.
            // Other points are on two segments so this wouldn't work for them.
            Vector3 result2 = _trackPoints[pointIndex].position + LaneOffset(System.Math.Max(1, pointIndex), lane) + _playerOffset;
            //Debug.Log("first result2: " + result2);
            return result2; 
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
            Vector3 result2 = _trackPoints[pointIndex].position + LaneOffset(pointIndex, lane) + _playerOffset;
            //Debug.Log("result2: " + result2);
            return result2;
        }

        Vector2 lanePointA = (_trackPoints[pointIndex - 1].position + laneOffset).To2D();
        Vector2 lanePointB = (_trackPoints[pointIndex].position + laneOffset).To2D();

        Vector2 nextLanePointA = (_trackPoints[pointIndex].position + nextLaneOffset).To2D();
        Vector2 nextLanePointB = (_trackPoints[pointIndex + 1].position + nextLaneOffset).To2D();

        Vector3 result = VectorUtils.LinesIntersectionPoint2D(lanePointA, lanePointB, nextLanePointA, nextLanePointB).To3D();
        result.y = _trackPoints[pointIndex].position.y;
        result += _playerOffset;

        //if (float.IsNaN(result.x))
        //{
        //    // result is NaN. (1.50, 52.50) (1.50, 22.50) (-1.50, 22.50) (-1.50, 32.50)
        //    Debug.Log("pointIndex: " + pointIndex + ". result is NaN. " + lanePointA + " " + lanePointB + " " + nextLanePointA + " " + nextLanePointB
        //        + " difference: " + difference.sqrMagnitude);
        //}
        if (test)
            Debug.Log($"intersection point: {result}, track point at start of segment: {pointIndex - 1}" +
                $", track point at end of segment and start of next segment: {pointIndex}, track point at end of next segment: {pointIndex + 1}");

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

    //public Vector3 ClosestPositionAlongTrackIgnoringVertical(Vector3 position, int nextPointIndex, out int closestSegmentEndpointIndex)
    //{
    //    Vector2 closestOnPriorSegment, closestOnCurrentSegment, closestOnNextSegment;

    //    if (nextPointIndex < 2)
    //        closestOnPriorSegment = Vector2.positiveInfinity;
    //    else
    //        closestOnPriorSegment = ClosestPointAlongTrackSegment(nextPointIndex - 1, position);

    //    closestOnCurrentSegment = ClosestPointAlongTrackSegment(nextPointIndex, position);


    //    if (nextPointIndex == NumPoints - 1)
    //        closestOnNextSegment = Vector2.positiveInfinity;
    //    else
    //        closestOnNextSegment = ClosestPointAlongTrackSegment(nextPointIndex + 1, position);

    //    Debug.Log("closest points: " + closestOnPriorSegment + " " + closestOnCurrentSegment + " " + closestOnNextSegment);

    //    float sqrDistanceToPriorSegment = (closestOnPriorSegment - position.To2D()).magnitude;
    //    float sqrDistanceToCurrentSegment = (closestOnCurrentSegment - position.To2D()).magnitude;
    //    float sqrDistanceToNextSegment = (closestOnNextSegment - position.To2D()).magnitude;

    //    if (sqrDistanceToPriorSegment < Mathf.Min(sqrDistanceToCurrentSegment, sqrDistanceToNextSegment))
    //    {
    //        closestSegmentEndpointIndex = nextPointIndex - 1;
    //        return closestOnPriorSegment.To3D();
    //    }
    //    else if (sqrDistanceToCurrentSegment < Mathf.Min(sqrDistanceToPriorSegment, sqrDistanceToNextSegment))
    //    {
    //        closestSegmentEndpointIndex = nextPointIndex;
    //        return closestOnCurrentSegment.To3D();
    //    }
    //    else
    //    {
    //        closestSegmentEndpointIndex = nextPointIndex + 1;
    //        return closestOnNextSegment.To3D();
    //    }
    //}

    //public Vector2 ClosestPointAlongTrackSegment(int segmentEndpointIndex, Vector3 position)
    //{
    //    return VectorUtils.ClosestPointOnLineSegment2D(position.To2D(), TrackPoint(segmentEndpointIndex - 1).To2D(), TrackPoint(segmentEndpointIndex).To2D());
    //}
}
