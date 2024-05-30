using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    //public Vector3 ClosestPointOnStoredLane(Vector3 point)
    //{
    //    return BezierCurve(TOfClosestPointOnStoredLane(point));
    //}

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

    public float Lane(Vector3 currentPosition, out float t)
    {
        // Find the closest point on the middle lane
        StoreLane(0);
        t = TOfClosestPointOnStoredLane(currentPosition);
        Vector3 closestPointOnMiddleLane = BezierCurve(t);

        // Determine whether the point is to the left or right of the middle lane
        Vector3 curveDerivative = BezierCurveDerivative(t); // direction the curve is going at this point
        bool toLeft = VectorUtils.PointIsToLeftOfVector(closestPointOnMiddleLane, closestPointOnMiddleLane + curveDerivative, currentPosition);

        // Find the distance to that point, but only along the direction which is perpendicular to the curve
        Vector2 offset = (currentPosition - closestPointOnMiddleLane).To2D();
        Vector2 perpendicularDirection = Vector2.Perpendicular(BezierCurveDerivative(t).To2D());
        if (toLeft)
            perpendicularDirection *= -1;
        float distanceToMiddleLane = VectorUtils.ProjectionMagnitude(offset, perpendicularDirection);

        float lane = toLeft ? distanceToMiddleLane : -distanceToMiddleLane;
        lane /= TrackPositions.Instance.DistanceBetweenLanes;
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

    //public (Vector2, Vector2) IntersectionsOfBezierCurveWithLine2D(Vector2 linePoint1, Vector2 linePoint2)
    //{
    //    // https://www.tumblr.com/floorplanner-techblog/66681002205/computing-the-intersection-between-linear-and

    //    Vector2 A = linePoint1;
    //    Vector2 B = linePoint2;
    //    Vector2 C = p0.To2D();
    //    Vector2 D = p1.To2D();
    //    Vector2 E = p2.To2D();

    //    if (StartTransform.forward.To2D() == EndTransform.forward.To2D())
    //    {
    //        // The bezier curve is a straight line, so handle this differently
    //        Vector2 result = VectorUtils.LineLineIntersection2d(A, B, C, E);
    //        return (result, result);
    //    }

        


    //    float k = (A.y - B.y) / (B.x - A.x);
    //    float a = k * (C.x - 2 * D.x + E.x) + C.y - 2 * D.y + E.y;
    //    float b = -2 * (k * (C.x - D.x) + C.y - D.y);
    //    float c = k * (C.x - A.x) + C.y - A.y;

    //    if (Mathf.Abs(B.x - A.x) < .00001f)
    //    {
    //        // The line is vertical (x = constant) so need to use a different method.
    //        a = C.x - 2 * D.x + E.x;
    //        b = -2 * (C.x - D.x);
    //        c = C.x - A.x;
    //    }

    //    if (Mathf.Abs(B.y - A.y) < .00001f)
    //    {
    //        // The line is horizontal (y = constant) so need to use a different method.

    //        a = C.y - 2 * D.y + E.y;
    //        b = -2 * (C.y - D.y);
    //        c = C.y - A.y;
    //    }

    //    float triangle = b * b - 4 * a * c; // probably only do 1 sqrt on this.

    //    if (triangle < 0)
    //    {
    //        Debug.LogError("no intersections");
    //        // no intersections
    //        return (new Vector2(float.NaN, float.NaN), new Vector2(float.NaN, float.NaN));
    //    }

    //    float t1 = (-b + Mathf.Sqrt(triangle)) / (2 * a);
    //    float t2 = (-b - Mathf.Sqrt(triangle)) / (2 * a);

    //    Vector2 option1 = BezierCurve(t1).To2D();
    //    Vector2 option2 = BezierCurve(t2).To2D();

    //    return (option1, option2);
    //}

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
       

        //for (int i = 0; i < _test.Length; i++)
        //{
        //    Gizmos.color = i < 3 ? Color.red : Color.green;
        //    Vector3 pos = new Vector3(_test[i].x, 3f, _test[i].y);
        //    Gizmos.DrawSphere(pos, .1f);
        //}
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
