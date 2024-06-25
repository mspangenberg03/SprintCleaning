using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorUtils : MonoBehaviour
{

    public static Vector3 RotateVectorAroundYAxis(Vector3 direction, float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);
        float x = cos * direction.x - sin * direction.z;
        float z = sin * direction.x + cos * direction.z;
        return new Vector3(x, direction.y, z);
    }

    // This doesn't handle cases where the lines are parallel or colinear. The stackoverflow link explains how to handle those cases.
    public static Vector2 LinesIntersectionPoint2D(Vector2 line1PointA, Vector2 line1PointB, Vector2 line2PointA, Vector2 line2PointB)
    {
        // Find where those two lines intersect (on the x-z plane)
        // https://stackoverflow.com/questions/563198/how-do-you-detect-where-two-line-segments-intersect 
        // except allow t to be any value
        Vector2 p = line1PointA;
        Vector2 r = line1PointB - p;
        Vector2 q = line2PointA;
        Vector2 s = line2PointB - line2PointA;

        float t = Cross2D(q - p, s) / Cross2D(r, s); // t = (q − p) x s / (r x s)
        return p + t * r;

        // two dimensional cross product like in the link
        float Cross2D(Vector2 v, Vector2 w) => v.x * w.y - v.y * w.x;
    }

    public static bool PointIsToLeftOfVector(Vector3 vectorStart, Vector3 vectorEnd, Vector3 point)
    {
        // https://discussions.unity.com/t/check-if-a-point-is-on-the-right-or-left-of-a-vector/180869
        return Vector3.Cross(vectorEnd - vectorStart, point - vectorStart).y < 0;
    }

    public static bool VelocityWillOvershoot(Vector3 velocity, Vector3 currentPosition, Vector3 targetPosition, float deltaTime)
    {
        // If the direction to the target will become opposite, then it's about to get past the next point
        Vector3 currentDisplacement = currentPosition - targetPosition;
        Vector3 nextDisplacement = currentDisplacement + velocity * deltaTime;
        return Vector3.Dot(currentDisplacement, nextDisplacement) <= 0.0001f;
    }

    public static float ProjectionMagnitude(Vector2 vector, Vector2 projectOnto)
    {
        return Vector2.Dot(vector, projectOnto.normalized);
    }

    public static bool TwoPointsAreOnDifferentSidesOfPlane(Vector3 point1, Vector3 point2, Vector3 planePoint, Vector3 planeNormalDirection)
    {
        // http://mathonline.wikidot.com/point-normal-form-of-a-plane
        // equation of a plane: ax + by + cz + d = 0
        // where the vector <a, b, c> is normal to the plane
        // and d = ax_0+by_0+cz_0 where <x_0, y_0, z_0> is a point on the plane

        float d = -Vector3.Dot(planePoint, planeNormalDirection);

        // https://stackoverflow.com/questions/15688232/check-which-side-of-a-plane-points-are-on
        float value1 = Vector3.Dot(planeNormalDirection, point1) + d;
        float value2 = Vector3.Dot(planeNormalDirection, point2) + d;
        return Mathf.Sign(value1) != Mathf.Sign(value2);

    }

    public static Vector2 ClosestPointOnLine2D(Vector2 point, Vector2 linePoint1, Vector2 linePoint2)
    {
        return ClosestPointOnLineOrSegment2D(point, linePoint1, linePoint2, false);
    }

    public static Vector2 ClosestPointOnSegment2D(Vector2 point, Vector2 segmentPoint1, Vector2 segmentPoint2)
    {
        return ClosestPointOnLineOrSegment2D(point, segmentPoint1, segmentPoint2, true);
    }

    public static Vector2 ClosestPointOnLineOrSegment2D(Vector2 point, Vector2 segmentPoint1, Vector2 segmentPoint2, bool segment)
    {
        // stack overflow shortest distance between a point and a line segment
        Vector2 a = segmentPoint1;
        Vector2 b = segmentPoint2;

        Vector2 bMinusA = b - a;
        Vector2 pointMinusA = point - a;

        float sqrLength = bMinusA.sqrMagnitude;
        if (sqrLength == 0)
            return a;

        float t = Vector2.Dot(pointMinusA, bMinusA) / sqrLength;
        if (segment)
            t = Mathf.Clamp01(t);
        return a + t * bMinusA;
    }

    public static bool PolygonsOverlap2D(Vector2[] a, Vector2[] b)
    {
        // This only works for convex polygons. The points need to be in clockwise or counterclockwise order.
        // https://stackoverflow.com/questions/10962379/how-to-check-intersection-between-2-rotated-rectangles

        for (int i = 0; i < 2; i++)
        {
            Vector2[] currentPolygon = i == 0 ? a : b;
            for (int j = 0; j < currentPolygon.Length; j++)
            {
                int nextIndex = (j + 1) % currentPolygon.Length;

                Vector2 normal = Vector2.Perpendicular(currentPolygon[j] - currentPolygon[nextIndex]);

                CalculateMinAndMax(a, out float minA, out float maxA);
                CalculateMinAndMax(b, out float minB, out float maxB);
                void CalculateMinAndMax(Vector2[] polygon, out float min, out float max)
                {
                    min = float.PositiveInfinity;
                    max = float.NegativeInfinity;
                    for (int k = 0; k < polygon.Length; k++)
                    {
                        float projection = Vector2.Dot(normal, polygon[k]);
                        min = Math.Min(min, projection);
                        max = Math.Max(max, projection);
                    }
                }

                if (maxA < minB || maxB < minA)
                    return false;
            }
        }
        return true;
    }

}
