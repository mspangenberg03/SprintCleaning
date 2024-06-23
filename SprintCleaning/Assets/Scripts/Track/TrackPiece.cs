using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Use a bezier curve as the path between the start and end
// https://en.wikipedia.org/wiki/B%C3%A9zier_curve#Quadratic_B%C3%A9zier_curves

public class TrackPiece : MonoBehaviour, PoolOfMonoBehaviour<TrackPiece>.IPoolable
{
    public const int TRACK_PIECE_LENGTH = 64;
    [field: SerializeField] public Transform StartTransform { get; private set; } // Used for positioning this track piece when creating it
    [field: SerializeField] public Transform EndTransform { get; private set; } // Used for the player's target

    private Vector3 p0; // start of bezier curve
    private Vector3 p1; // control point of bezier curve
    private Vector3 p2; // end point of bezier curve

    private static List<Vector2> _calcIntersections = new();
    private static List<float> _calcRoots = new();

    public TrackPiece Prior { get; private set; }
    public TrackPiece Next { get; private set; }

    public List<Garbage> GarbageOnThisTrackPiece { get; set; } = new();
    public List<Building> BuildingsByThisTrackPiece { get; set; } = new();


    public void InitializeUponPrefabInstantiated(PoolOfMonoBehaviour<TrackPiece> poolOfMonoBehaviour) { }

    public void InitializeUponProducedByPool()
    {
        List<TrackPiece> currentTrackPieces = TrackGenerator.Instance.TrackPieces;
        if (currentTrackPieces.Count > 0)
        {
            Prior = TrackGenerator.Instance.TrackPieces[^1];
            Prior.Next = this;
        }
    }

    public void OnReturnToPool()
    {
        foreach (Garbage x in GarbageOnThisTrackPiece)
            x.ReturnToPool();
        GarbageOnThisTrackPiece.Clear();

        foreach (Building b in BuildingsByThisTrackPiece)
            b.ReturnToPool();
        BuildingsByThisTrackPiece.Clear();

        if (Next != null)
            Next.Prior = null;
        Next = null;
        Prior = null;
    }

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

    public Vector3 BezierCurve(float t)
    {
        // A point on the track piece's bezier curve
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
            p1z + (1 - t) * (1 - t) * (p0z - p1z) + t * t * (p2z - p1z));
    }

    public Vector3 BezierCurveDerivative(float t)
    {
        return 2 * (1 - t) * (p1 - p0) + 2 * t * (p2 - p1);
    }

    public Vector3 BezierCurveSecondDerivative()
    {
        return 2 * (p2 - 2 * p1 + p0);
    }

    public bool IntersectsWithLine2D(Vector2 linePoint1, Vector2 linePoint2)
    {
        // https://stackoverflow.com/questions/27664298/calculating-intersection-point-of-quadratic-bezier-curve

        Vector2 P0 = p0.To2D();
        Vector2 P1 = p1.To2D();
        Vector2 P2 = p2.To2D();

        _calcIntersections.Clear();
        _calcRoots.Clear();
        List<Vector2> intersections = _calcIntersections;
        List<float> roots = _calcRoots;

        Vector2 a1 = linePoint1;
        Vector2 a2 = linePoint2;

        Vector2 normal = new Vector2(a1.y - a2.y, a2.x - a1.x);
        Vector2 c2 = new Vector2(P0.x - 2 * P1.x + P2.x, P0.y - 2 * P1.y + P2.y); 
        Vector2 c1 = new Vector2(-2 * P0.x + 2 * P1.x , -2 * P0.y + 2 * P1.y); 
        Vector2 c0 = new Vector2(P0.x, P0.y);

        float coefficient = a1.x * a2.y - a2.x * a1.y;
        float a = normal.x * c2.x + normal.y * c2.y;
        float b = (normal.x * c1.x + normal.y * c1.y) / a;
        float c = (normal.x * c0.x + normal.y * c0.y + coefficient) / a;

        float d = b * b - 4 * c;


        if (d > 0)
        {
            float e = Mathf.Sqrt(d);
            roots.Add((-b + e) / 2);
            roots.Add((-b - e) / 2);
        }
        else if (d == 0)
            roots.Add(-b / 2);

        for (int i = 0; i < roots.Count; i++)
        {
            float minX = Mathf.Min(a1.x, a2.x); 
            float minY = Mathf.Min(a1.y, a2.y); 
            float maxX = Mathf.Max(a1.x, a2.x); 
            float maxY = Mathf.Max(a1.y, a2.y); 
            float t = roots[i]; 
            if (t >= 0 && t <= 1)
            {
                Vector2 point = BezierCurve(t).To2D();
                float x = point.x;
                float y = point.y;
                if (a1.x == a2.x && y >= minY && y <= maxY)
                    intersections.Add(point);
                else if (a1.y == a2.y && c >= minX && x <= maxX)
                    intersections.Add(point);
                else if (x >= minX && y >= minY && x <= maxX && y <= maxY)
                    intersections.Add(point);
            }
        }

        return intersections.Count > 0;
    }

    public float ApproximateLengthForStoredLane()
    {
        const int steps = 100;
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
        Gizmos.color = new Color(1f, 1f, 1f, .3f);
        DrawOneLane(-1f);
        DrawOneLane(0f);
        DrawOneLane(1f);

        float alpha = Application.isPlaying ? .2f : 1f;
        Gizmos.color = new Color(0f, 0f, 0f, alpha);
        float t = 0;
        for (int i = 0; i < TRACK_PIECE_LENGTH; i++)
        {
            StoreLane(0);
            t = FindTForDistanceAlongStoredLane(1f, t);
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

    public float FindTForDistanceAlongStoredLane(float extraDistanceAlongTrack, float startingFromT)
    {
        const int segments = 1000;
        float totalDistance = 0;
        Vector3 priorPosition = BezierCurve(startingFromT);
        float priorDistance = 0f;
        float priorT = 0;
        for (int i = 1; i <= segments; i++)
        {
            float nextT = Mathf.Lerp(startingFromT, 1f, (float)i / segments);
            Vector3 nextPosition = BezierCurve(nextT);
            totalDistance += (nextPosition - priorPosition).magnitude;
            if (totalDistance >= extraDistanceAlongTrack)
            {
                float inverseLerp = Mathf.InverseLerp(priorDistance, totalDistance, extraDistanceAlongTrack);
                return Mathf.Lerp(priorT, nextT, inverseLerp);
            }
            priorPosition = nextPosition;
            priorDistance = totalDistance;
            priorT = nextT;
        }

        return -1;
    }
}
