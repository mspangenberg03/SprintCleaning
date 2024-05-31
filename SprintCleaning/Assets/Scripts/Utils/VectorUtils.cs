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
}
