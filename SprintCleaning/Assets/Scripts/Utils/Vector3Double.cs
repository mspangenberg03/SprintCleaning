using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct Vector3Double
{
    public double x;
    public double y;
    public double z;

    public Vector3Double(double x, double y, double z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Vector3Double((double x, double y, double z) tuple)
    {
        x = tuple.x;
        y = tuple.y;
        z = tuple.z;
    }



    public double magnitude => System.Math.Sqrt(sqrMagnitude);
    public double sqrMagnitude => x * x + y * y + z * z;

    public Vector3Double normalized
    {
        get
        {
            double k = magnitude;
            return new Vector3Double(x / k, y / k, z / k);
        }
    }

    public Vector3Double normalizedSafely
    {
        get
        {
            double k = magnitude;
            return (k == 0) ? (zero) : new Vector3Double(x / k, y / k, z / k);
        }
    }

    public static Vector3Double zero => new Vector3Double(0, 0, 0);

    public static explicit operator Vector3Double(Vector3 v) => new Vector3Double(v.x, v.y, v.z);
    public static explicit operator Vector3(Vector3Double v) => new Vector3((float)v.x, (float)v.y, (float)v.z);

    public static Vector3Double operator /(Vector3Double v, double d) => new Vector3Double(v.x / d, v.y / d, v.z / d);
    public static Vector3Double operator *(Vector3Double v, double d) => new Vector3Double(v.x * d, v.y * d, v.z * d);
    public static Vector3Double operator *(double d, Vector3Double v) => new Vector3Double(v.x * d, v.y * d, v.z * d);
    public static Vector3Double operator +(Vector3Double v1, Vector3Double v2) => new Vector3Double(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
    public static Vector3Double operator -(Vector3Double v1, Vector3Double v2) => new Vector3Double(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
    public static Vector3Double operator -(Vector3Double v) => new Vector3Double(-v.x, -v.y, -v.z);


    public static double Dot(Vector3Double a, Vector3Double b) => a.x * b.x + a.y * b.y + a.z * b.z;
    public double Dot(Vector3Double other) => x * other.x + y * other.y + z * other.z;
    public double Dot(Vector3 other) => x * other.x + y * other.y + z * other.z;




    public override string ToString()
    {
        return "(" + x + ", " + y + ", " + z + ")";
    }

}