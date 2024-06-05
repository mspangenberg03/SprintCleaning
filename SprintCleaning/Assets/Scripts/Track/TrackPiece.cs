using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackPiece : MonoBehaviour
{
    public const int TRACK_PIECE_LENGTH = 16;
    [field: SerializeField] public Transform StartTransform { get; private set; } // Used for positioning this track piece when creating it
    [field: SerializeField] public Transform EndTransform { get; private set; } // Used for the player's target

    private Vector3 p0; // start of bezier curve
    private Vector3 p1; // control point of bezier curve
    private Vector3 p2; // end point of bezier curve

    public Vector3 EndPositionForStoredLane => p2;

    public void StoreLane(float lane)
    {
        Vector3 startDirection = StartTransform.forward;
        Vector3 endDirection = EndTransform.forward;
        p0 = StartTransform.position + lane * PlayerMovement.Settings.DistanceBetweenLanes * StartTransform.right;
        p2 = EndTransform.position + lane * PlayerMovement.Settings.DistanceBetweenLanes * EndTransform.right;

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

    public float FindTForClosestPointOnMidline(Vector3 point)
    {
        StoreLane(0);
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

    public float Lane(Vector3 currentPosition, float t)
    {
        // Find the closest point on the middle lane
        
        Vector3 closestPointOnMiddleLane = BezierCurve(t);

        // Find the distance to that point, but only along the direction which is perpendicular to the curve, because t uses
        // an approximation so it's a bit off.
        Vector2 offset = (currentPosition - closestPointOnMiddleLane).To2D();
        Vector2 perpendicularDirection = Vector2.Perpendicular(BezierCurveDerivative(t).To2D());
        bool toLeft = VectorUtils.PointIsToLeftOfVector(closestPointOnMiddleLane, closestPointOnMiddleLane + BezierCurveDerivative(t), currentPosition);
        if (toLeft)
            perpendicularDirection *= -1;
        float distanceToMiddleLane = VectorUtils.ProjectionMagnitude(offset, perpendicularDirection);

        float lane = toLeft ? distanceToMiddleLane : -distanceToMiddleLane;
        lane /= PlayerMovement.Settings.DistanceBetweenLanes;
        return lane;
    }

    public Vector3 BezierCurve(float t)
    {
        // Use a bezier curve as the path between the start and end of the current track piece.
        // https://en.wikipedia.org/wiki/B%C3%A9zier_curve#Quadratic_B%C3%A9zier_curves

        // A point on the bezier curve for the current track piece.
        return p1 + (1 - t) * (1 - t) * (p0 - p1) + t * t * (p2 - p1);
    }

    public Vector3 BezierCurveDerivative(float t)
    {
        return 2 * (1 - t) * (p1 - p0) + 2 * t * (p2 - p1);
    }

    public Vector3 BezierCurveSecondDerivative()
    {
        return 2 * (p2 - 2 * p1 + p0);
    }

    public float ApproximateCurveLength()
    {
        const int steps = 100;
        float result = 0;
        Vector3 priorPoint = BezierCurve(0);
        for (int i = 1; i <= steps; i++)
        {
            float t = ((float)i) / steps;
            Vector3 nextPoint = BezierCurve(t);
            result += (nextPoint - priorPoint).magnitude;
            priorPoint = nextPoint;
        }
        return result;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        DrawOneLane(-1f);
        Gizmos.color = Color.black;
        DrawOneLane(0f);
        Gizmos.color = Color.blue;
        DrawOneLane(1f);

        Gizmos.color = new Color(.5f, 0f, .5f, .1f);
        for (int i = 0; i < TRACK_PIECE_LENGTH; i++)
        {
            DrawPerpendicularLine(i);
        }
    }

    private void DrawOneLane(float lane)
    {
        StoreLane(lane);
        // Use a bezier curve as the path between the start and end of the current track piece.
        // https://en.wikipedia.org/wiki/B%C3%A9zier_curve#Quadratic_B%C3%A9zier_curves
        const int segments = 100;
        Vector3 verticalOffset = Vector3.up * PlayerMovement.Settings.PlayerVerticalOffset;
        Vector3 priorPoint = p0 + verticalOffset;
        for (int i = 1; i <= segments; i++)
        {
            float t = (float)i / segments;
            Vector3 nextPoint = BezierCurve(t) + verticalOffset;
            Gizmos.DrawLine(priorPoint, nextPoint);
            priorPoint = nextPoint;
        }
    }

    private void DrawPerpendicularLine(int distanceAlongTrack)
    {
        StoreLane(0);
        float t = (float)distanceAlongTrack / TRACK_PIECE_LENGTH; // opposite of distanceAlongTrack. doesnt matter
        Vector3 position = BezierCurve(t) + PlayerMovement.Settings.PlayerVerticalOffset * Vector3.up;
        Vector3 direction = Vector2.Perpendicular(BezierCurveDerivative(t).To2D()).To3D().normalized * PlayerMovement.Settings.DistanceBetweenLanes;
        Gizmos.DrawLine(position - direction, position + direction);
    }
}
