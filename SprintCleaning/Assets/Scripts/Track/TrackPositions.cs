using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// could make this a monobehavior if stuff besides PlayerMovement needs it

/// <summary>
/// Represents the track and has methods to get positions along the lanes.
/// </summary>
public class TrackPositions : MonoBehaviour
{

    [SerializeField] private float _distanceBetweenLanes;
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

    /// <summary>
    /// A point along the track at a lane.
    /// </summary>
    public Vector3 LanePoint(int pointIndex, float lane)
    {
        if (pointIndex == 0 || pointIndex == TrackGenerator.Instance.TrackPieces.Count - 1)
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

        Vector2 lanePointA = (TrackPoint(pointIndex - 1) + laneOffset).To2D();
        Vector2 lanePointB = (TrackPoint(pointIndex) + laneOffset).To2D();

        Vector2 nextLanePointA = (TrackPoint(pointIndex) + nextLaneOffset).To2D();
        Vector2 nextLanePointB = (TrackPoint(pointIndex + 1) + nextLaneOffset).To2D();

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
        Vector3 pathDirection = TrackPoint(laneSegmentEndpointIndex) - TrackPoint(laneSegmentEndpointIndex - 1);
        float trackAngle = Vector3.SignedAngle(pathDirection, Vector3.forward, Vector3.up);

        Vector3 laneOffset = lane * _distanceBetweenLanes * Vector3.right; // the offset if the track isn't rotated
        return VectorUtils.RotateVectorAroundYAxis(laneOffset, trackAngle);
    }

    //public float ConvertPositionToLane(int endpointIndex, Vector3 position)
    //{
    //    // This and probably other things need to take into account the bezier curves. So gotta implement the approximation
    //    // for closest point on a bezier curve (and the lane equals the distance excluding y from that closest point divided by space between lanes)
    //    Vector2 closestPoint = ClosestPointOnTrack(endpointIndex, position);
    //    float distance = (closestPoint - position.To2D()).magnitude;

    //    bool toLeft = VectorUtils.PointIsToLeftOfVector(TrackPoint(endpointIndex - 1), TrackPoint(endpointIndex), position - Vector3.up * _playerVerticalOffset);
    //    float sign = toLeft ? -1f : 1f;
    //    return distance / _distanceBetweenLanes * sign;
    //}

    private Vector3 PositionToBeOnLaneUsingStraightLines(int endpointIndex, float lane, Vector3 position)
    {
        Vector2 targetLaneStart = LanePoint(endpointIndex - 1, lane).To2D();
        Vector2 targetLaneEnd = LanePoint(endpointIndex, lane).To2D();

        Vector3 result = VectorUtils.ClosestPointOnSegment2D(position.To2D(), targetLaneStart, targetLaneEnd).To3D();
        result.y = position.y;
        return result;
    }

    //public Vector3 ClosestPointOnLane(int endpointIndex, float lane, Vector3 position)
    //{

    //    return PositionToBeOnLaneUsingStraightLines(endpointIndex, lane, position);

    //    // to do

    //    //Transform start = StartTransform(endpointIndex);
    //    //Transform end = EndTransform(endpointIndex);

    //    //Vector3 startDirection = start.forward;
    //    //Vector3 startPosition = start.position + lane * DistanceBetweenLanes * start.right;

    //    //Vector3 endDirection = end.forward;
    //    //Vector3 endPosition = end.position + lane * DistanceBetweenLanes * end.right;

    //    //bool trackPieceGoesStraight = Vector3.Angle(startDirection, endDirection) < .1f;
    //    //bool closeEnoughToGoStraightToEnd = (position - endPosition).magnitude < 1.25f * PlayerMovement.Settings._playerSpeed * Time.deltaTime;

    //    //if (trackPieceGoesStraight || closeEnoughToGoStraightToEnd)
    //    //{
    //    //    return PositionToBeOnLaneUsingStraightLines(endpointIndex, lane, position);
    //    //}

    //    //// probably move bezier stuff into TrackPositions
    //    //// (this code was copy and pasted from player movement)


    //    //// Use a bezier curve as the path between the start and end of the current track piece.
    //    //// https://en.wikipedia.org/wiki/B%C3%A9zier_curve#Quadratic_B%C3%A9zier_curves
    //    //Vector3 p0 = startPosition;
    //    //Vector3 p2 = endPosition;
    //    //Vector3 p1 = VectorUtils.LinesIntersectionPoint2D(p0.To2D(), p0.To2D() + startDirection.To2D(), p2.To2D(), p2.To2D() + endDirection.To2D()).To3D();
    //    //p1.y = (p0.y + p2.y) / 2;

    //    //Vector3 r = position;

    //    //const int steps = 1000;
    //    //float bestT = 0;
    //    //float bestError = float.PositiveInfinity;
    //    //for (int i = 0; i < steps; i++)
    //    //{
    //    //    Vector3 BMinusR = PlayerMovement.BezierCurve((float)i / steps, p0, p1, p2) - r;

    //    //    float error = BMinusR.sqrMagnitude;

    //    //    if (error < bestError)
    //    //    {

    //    //        bestT = (float)i / steps;
    //    //        bestError = error;
    //    //    }
    //    //}

    //    //if (bestError == float.PositiveInfinity)
    //    //{
    //    //    return PositionToBeOnLaneUsingStraightLines(endpointIndex, lane, position);
    //    //}

    //    //return PlayerMovement.BezierCurve(bestT, p0, p1, p2);
    //}
}
