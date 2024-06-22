using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ShowStuff : MonoBehaviour
{
    public Transform trackPieceTransform;
    public Mesh makeFrom;

    private void OnDrawGizmos()
    {
        if (trackPieceTransform == null)
            return;
        if (makeFrom == null)
            return;
        Gizmos.color = Color.magenta;
        Matrix4x4 localToWorld = trackPieceTransform.localToWorldMatrix;
        for (int i = 0; i < makeFrom.vertexCount; i++)
        {
            Vector3 point = makeFrom.vertices[i];
            {
                point = localToWorld.MultiplyPoint3x4(point);
                Gizmos.DrawSphere(point, .03f);
            }
        }
    }
}

public class TrackPieceMeshMaker : ScriptableWizard
{
    //[SerializeField] private Mesh _makeFrom;
    [SerializeField] private string _makeFromMeshWithName = "Pavement";
    [SerializeField] private GameObject _trackPiecePrefab;
    [SerializeField] private string _saveName = "";

    

    [MenuItem("Custom/Track Piece Mesh Maker")]
    public static void ShowMaker()
    {
        DisplayWizard<TrackPieceMeshMaker>("Track Piece Mesh Maker", "Make Mesh");

        
    }

    

    private void OnWizardCreate()
    {
        string[] guids = AssetDatabase.FindAssets(_makeFromMeshWithName);
        if (guids.Length != 1)
        {
            Debug.LogError("Found a bad number of assets: " + guids.Length);
        }
        Mesh makeFrom = AssetDatabase.LoadAssetAtPath<Mesh>(AssetDatabase.GUIDToAssetPath(guids[0]));

        Debug.Log(makeFrom.triangles.Length + " " + makeFrom.vertexCount);

        GameObject g = new GameObject();
        g.AddComponent<ShowStuff>();
        g.GetComponent<ShowStuff>().makeFrom = makeFrom;

        //Mesh result = MakeMesh();
        //result.Optimize();
        //result.UploadMeshData(true);
        //SaveMesh(result);


    }

    private void SaveMesh(Mesh m)
    {
        string path = "Assets/GeneratedAssets/" + _saveName + ".asset";
        Debug.Log("Saving new mesh.");
        AssetDatabase.CreateAsset(m, path);
    }

    //private Mesh MakeMesh()
    //{
    //    return MeshCreatorMethods.MakeMesh(vertices, uv, triangles);
    //}

    private string RandomStringForDefaultSaveName()
    {
        string result = "New Generated Mesh ";
        for (int i = 0; i < 5; i++)
            result += (char)Random.Range(97, 123);
        return result;
    }
}



//// See http://www.code-spot.co.za/2020/11/04/generating-meshes-procedurally-in-unity/

//public static class MeshCreatorMethods
//{


//    public static Mesh MakeMesh(Vector3[] vertices, int[] triangles)
//    {
//        // triangles are triples of indexes in the vertices array. 
//        // triangle indexes are ordered such that the triangle's vertices are in a clockwise order relative to the camera (otherwise they aren't front-facing so not rendered.

//        // uv corresponds to each vertex. it's the point on a texture from 0 to 1, and each mesh triangle warps the triangle of uv points on the texture to fit onto its world-space triangle.
//        // I guess I used uv in circular shaders b/c a quad is just a whole texture's coordinate space.
//        // It'll matter when I make meshes to optimize circular shaders, so make uv 0 to 1 containing the vertices. Won't matter for the shapes w/o fragment shader clipping.
//        Vector2[] uv = ShiftAndScaleToFitIn0To1(vertices);

//        // rescale to make it have width/height of 1 (without warping)
//        (float minX, float maxX, float minY, float maxY) bounds = MinAndMaxXAndY(vertices);
//        float maxSize = Mathf.Max(bounds.maxX - bounds.minX, bounds.maxY - bounds.minY);
//        float rescale = 1f / maxSize;
//        for (int i = 0; i < vertices.Length; i++)
//            vertices[i] *= rescale;


//        return new Mesh
//        {
//            vertices = vertices,
//            uv = uv,
//            triangles = triangles
//        };
//    }

//    public static Mesh MakeMesh(Vector3[] vertices, Vector2[] uv, int[] triangles)
//    {
//        return new Mesh
//        {
//            vertices = vertices,
//            uv = uv,
//            triangles = triangles
//        };
//    }

//    private static Vector2[] ShiftAndScaleToFitIn0To1(Vector3[] points)
//    {
//        // coordinate shift / scale to fit points in a square with bottom left (0, 0) and top right (1, 1). ignores the z coordinates.
//        if (points.Length == 0)
//            return new Vector2[0];
//        (float minX, float maxX, float minY, float maxY) bounds = MinAndMaxXAndY(points);

//        // rescale so the total width/height is 1. Except don't warp it, so just make the larger of those be 1.
//        float rescale = 1f / Mathf.Max(bounds.maxX - bounds.minX, bounds.maxY - bounds.minY);

//        Vector2[] result = new Vector2[points.Length];
//        for (int i = 0; i < result.Length; i++)
//        {
//            // shift points so the min x and min y are at 0, then multiply so the max x and max y are at 1.
//            float newX = (points[i].x - bounds.minX) * rescale;
//            float newY = (points[i].y - bounds.minY) * rescale;

//            result[i] = new Vector2(newX, newY);
//        }

//        return result;
//    }

//    private static (float minX, float maxX, float minY, float maxY) MinAndMaxXAndY(Vector3[] vertices)
//    {
//        float minX = float.PositiveInfinity;
//        float maxX = float.NegativeInfinity;
//        float minY = float.PositiveInfinity;
//        float maxY = float.NegativeInfinity;

//        foreach (Vector3 v in vertices)
//        {
//            minX = Mathf.Min(minX, v.x);
//            maxX = Mathf.Max(maxX, v.x);
//            minY = Mathf.Min(minY, v.y);
//            maxY = Mathf.Max(maxY, v.y);
//        }

//        return (minX, maxX, minY, maxY);
//    }

//}

