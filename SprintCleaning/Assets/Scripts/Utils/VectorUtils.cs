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
}
