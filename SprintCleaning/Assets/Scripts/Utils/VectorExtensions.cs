using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorExtensions 
{
    public static Vector2 To2D(this Vector3 vector) => new Vector2(vector.x, vector.z);

    public static Vector3 To3D(this Vector2 vector) => new Vector3(vector.x, 0, vector.y);

    public static string DetailedString(this Vector3 vector) => $"({vector.x}, {vector.y}, {vector.z})";
    public static string DetailedString(this Vector2 vector) => $"({vector.x}, {vector.y})";
}
