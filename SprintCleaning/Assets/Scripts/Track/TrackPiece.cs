using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackPiece : MonoBehaviour
{
    [field: SerializeField] public Transform StartTransform { get; private set; } // Used for positioning this track piece when creating it
    [field: SerializeField] public Transform EndTransform { get; private set; } // Used for the player's target

    private Vector3 p0; // start of bezier curve
    private Vector3 p1; // control point of bezier curve
    private Vector3 p2; // end point of bezier curve

    public Vector3 EndPosition => p2; // end position for current lane

    public void StoreLane(float lane)
    {
        Vector3 startDirection = StartTransform.forward;
        Vector3 endDirection = EndTransform.forward;
        p0 = StartTransform.position + lane * TrackPositions.Instance.DistanceBetweenLanes * StartTransform.right;
        p2 = EndTransform.position + lane * TrackPositions.Instance.DistanceBetweenLanes * EndTransform.right;

        if (Vector3.Angle(startDirection, endDirection) < .2f)
        { 
            p1 = (p0 + p2) / 2;
        }
        else
        {
            p1 = VectorUtils.LinesIntersectionPoint2D(p0.To2D(), p0.To2D() + startDirection.To2D(), p2.To2D(), p2.To2D() + endDirection.To2D()).To3D();
            p1.y = (p0.y + p2.y) / 2;
        }
    }

    public Vector3 ClosestPointOnStoredLane(Vector3 point)
    {
        return BezierCurve(TOfClosestPointOnStoredLane(point));
    }

    private float TOfClosestPointOnStoredLane(Vector3 point)
    {
        const int steps = 1000;
        float minSqrDistance = float.PositiveInfinity;
        float bestT = float.NaN;
        for (int i = 0; i <= steps; i++)
        {
            float t = ((float)i) / steps;
            Vector3 v = BezierCurve(t);
            if ((v - point).To2D().sqrMagnitude < minSqrDistance)
            {
                minSqrDistance = (v - point).To2D().sqrMagnitude;
                bestT = t;
            }
        }
        return bestT;
    }

    public Vector3 PointToMoveTowardsOnSameLane(Vector3 currentPosition, float distanceFromCurrentPosition)
    {
        // Find the closest point on the middle lane
        StoreLane(0);
        float t = TOfClosestPointOnStoredLane(currentPosition);
        Vector3 closestPointOnMiddleLane = BezierCurve(t);

        // Determine whether the point is to the left or right of the middle lane
        Vector3 curveDerivative = BezierCurveDerivative(t);
        bool toLeft = VectorUtils.PointIsToLeftOfVector(closestPointOnMiddleLane, closestPointOnMiddleLane + curveDerivative, currentPosition);

        // Find the distance to that point, but only along the direction which is perpendicular to the curve
        Vector2 offset = (currentPosition - closestPointOnMiddleLane).To2D();
        Vector2 perpendicularDirection = Vector2.Perpendicular(BezierCurveDerivative(t).To2D());
        if (toLeft)
            perpendicularDirection *= -1;
        float distanceToMiddleLane = VectorUtils.ProjectionMagnitude(offset, perpendicularDirection);

        // Store the lane of the point
        
        float lane = toLeft ? distanceToMiddleLane : -distanceToMiddleLane;
        lane /= TrackPositions.Instance.DistanceBetweenLanes;
        Debug.Log(lane);
        StoreLane(lane);

        // Find the t of currentPosition (does it change or can it just use the t from before?)
        // And then iterate starting from there to find a point on the curve at the specified distance away.
        // Need to start from there so it doesn't find an earlier point on the curve (which'd make the player move backwards).
        float startT = TOfClosestPointOnStoredLane(currentPosition);
        const int steps = 1000;
        float bestDistanceError = float.PositiveInfinity;
        float bestT = float.NaN;
        for (int i = 0; i <= steps; i++)
        {
            float nextT = Mathf.Lerp(startT, 1f, (float)i / steps);
            Vector3 nextPoint = BezierCurve(nextT);
            float distanceError = Mathf.Abs((currentPosition - nextPoint).magnitude - distanceFromCurrentPosition);
            if (distanceError < bestDistanceError)
            {
                bestT = nextT;
                bestDistanceError = distanceError;
            }
        }
        return BezierCurve(bestT);
    }

    private Vector3 BezierCurve(float t)
    {
        // Use a bezier curve as the path between the start and end of the current track piece.
        // https://en.wikipedia.org/wiki/B%C3%A9zier_curve#Quadratic_B%C3%A9zier_curves

        // A point on the bezier curve for the current track piece.
        return p1 + (1 - t) * (1 - t) * (p0 - p1) + t * t * (p2 - p1);
    }

    private Vector3 BezierCurveDerivative(float t)
    {
        return 2 * (1 - t) * (p1 - p0) + 2 * t * (p2 - p1);
    }

    private Vector3 BezierCurveSecondDerivative(float t)
    {
        return 2 * (p2 - 2 * p1 + p0);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;
        Gizmos.color = Color.green;
        DrawOneLane(-1f);
        Gizmos.color = Color.black;
        DrawOneLane(0f);
        Gizmos.color = Color.blue;
        DrawOneLane(1f);
       
    }

    private void DrawOneLane(float lane)
    {
        StoreLane(lane);
        // Use a bezier curve as the path between the start and end of the current track piece.
        // https://en.wikipedia.org/wiki/B%C3%A9zier_curve#Quadratic_B%C3%A9zier_curves
        const int segments = 100;
        Vector3 priorPoint = p0 + Vector3.up * TrackPositions.Instance.PlayerVerticalOffset;
        for (int i = 1; i <= segments; i++)
        {
            float t = (float)i / segments;
            Vector3 nextPoint = BezierCurve(t) + Vector3.up * TrackPositions.Instance.PlayerVerticalOffset;
            Gizmos.DrawLine(priorPoint, nextPoint);
            priorPoint = nextPoint;
        }
    }
}
