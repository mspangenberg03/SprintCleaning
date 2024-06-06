using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackPiece : MonoBehaviour
{
    public const int TRACK_PIECE_LENGTH = 16;
    [field: SerializeField] public Transform StartTransform { get; private set; } // Used for positioning this track piece when creating it
    [field: SerializeField] public Transform EndTransform { get; private set; } // Used for the player's target

    [Header("Info")]
    [SerializeField] private float _approximateLength;

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

    public (double, double, double) BezierCurveDouble(double t)
    {
        // cast to doubles
        (double p0x, double p0y, double p0z) = (p0.x, p0.y, p0.z);
        (double p1x, double p1y, double p1z) = (p1.x, p1.y, p1.z);
        (double p2x, double p2y, double p2z) = (p2.x, p2.y, p2.z);

        return (
            p1x + (1 - t) * (1 - t) * (p0x - p1x) + t * t * (p2x - p1x),
            p1y + (1 - t) * (1 - t) * (p0y - p1y) + t * t * (p2y - p1y),
            p1z + (1 - t) * (1 - t) * (p0z - p1z) + t * t * (p2z - p1z)
            );
    }

    public Vector3 BezierCurveDerivative(float t)
    {
        return 2 * (1 - t) * (p1 - p0) + 2 * t * (p2 - p1);
    }

    public Vector3 BezierCurveSecondDerivative()
    {
        return 2 * (p2 - 2 * p1 + p0);
    }

    public float ApproximateMidlineLength()
    {
        const int steps = 100;
        StoreLane(0);
        float sum = 0;
        Vector3 priorPoint = BezierCurve(0);
        for (int i = 1; i <= steps; i++)
        {
            float t = ((float)i) / steps;
            Vector3 nextPoint = BezierCurve(t);
            sum += (nextPoint - priorPoint).magnitude;
            priorPoint = nextPoint;
        }
        return sum;
    }

    public double ApproximateMidlineLengthForEditor(int steps)
    {
        StoreLane(0);
        double sum = 0;
        (double x, double y, double z) priorPoint = BezierCurveDouble(0);
        for (int i = 1; i <= steps; i++)
        {
            double t = ((double)i) / steps;
            (double x, double y, double z) nextPoint = BezierCurveDouble(t);
            (double dx, double dy, double dz) = (nextPoint.x - priorPoint.x, nextPoint.y - priorPoint.y, nextPoint.z - priorPoint.z);
            sum += System.Math.Sqrt(dx * dx + dy * dy + dz * dz);
            priorPoint = nextPoint;
        }
        return sum;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        DrawOneLane(-1f);
        Gizmos.color = Color.cyan;
        DrawOneLane(0f);
        Gizmos.color = Color.blue;
        DrawOneLane(1f);

        float alpha = Application.isPlaying ? .05f : 1f;
        Gizmos.color = new Color(0f, 0f, 0f, alpha);
        float t = 0;
        for (int i = 0; i < TRACK_PIECE_LENGTH; i++)
        {
            StoreLane(0);
            t = FindTForDistanceAlongTrack(1f, t);
            if (t == -1)
            {
                if (i == TRACK_PIECE_LENGTH - 1)
                    t = 1f;
                else
                    break;
            }
            Vector3 position = BezierCurve(t) + PlayerMovement.Settings.PlayerVerticalOffset * Vector3.up;
            Vector3 direction = Vector2.Perpendicular(BezierCurveDerivative(t).To2D()).To3D().normalized * PlayerMovement.Settings.DistanceBetweenLanes;
            Gizmos.DrawLine(position - direction, position + direction);
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

    private float FindTForDistanceAlongTrack(float extraDistanceAlongTrack, float startingFromT)
    {
        StoreLane(0);
        const int segments = 1000;
        float totalDistance = 0;
        Vector3 priorPosition = BezierCurve(startingFromT);
        for (int i = 1; i <= segments; i++)
        {
            float nextT = Mathf.Lerp(startingFromT, 1f, (float)i / segments);
            Vector3 nextPosition = BezierCurve(nextT);
            totalDistance += (nextPosition - priorPosition).magnitude;
            if (totalDistance >= extraDistanceAlongTrack)
                return nextT;
            priorPosition = nextPosition;
        }

        return -1;
    }
}
