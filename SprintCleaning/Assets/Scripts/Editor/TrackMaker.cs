using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public class TrackMaker : ScriptableWizard
{
    private const float TRACK_SEGMENT_PREFAB_LENGTH = 20;
    private const float TRACK_SEGMENT_PREFAB_WIDTH = 9.484299f;

    [SerializeField] private TrackPiece _trackPiece;
    [SerializeField] private int _numSegments = 32;

    private List<Vector3> _pointsLaneMinus1 = new();
    private List<Vector3> _pointsLane0 = new();
    private List<Vector3> _pointsLane1 = new();

    [MenuItem("Custom/Create Track")]
    public static void CreateWizard()
    {
        DisplayWizard<TrackMaker>("Create Track", "Create");
    }

    private void OnWizardCreate()
    {
        GameObject segmentPrefab = Resources.Load<GameObject>("Track Segment");

        for (int i = 0; i <= _numSegments; i++)
        {
            _trackPiece.StoreLane(0);
            float t = (float)i / _numSegments;
            if (t != 1)
                t = _trackPiece.FindTForDistanceAlongStoredLane(TrackPiece.TRACK_PIECE_LENGTH * t, 0);
            _pointsLane0.Add(_trackPiece.BezierCurve(t));
            float laneSize = TRACK_SEGMENT_PREFAB_WIDTH / PlayerMovement.Settings.DistanceBetweenLanes;
            _trackPiece.StoreLane(-laneSize);
            _pointsLaneMinus1.Add(_trackPiece.BezierCurve(t));
            _trackPiece.StoreLane(laneSize);
            _pointsLane1.Add(_trackPiece.BezierCurve(t));

        }

        Vector3 priorPointLaneMinus1 = _pointsLaneMinus1[0];
        Vector3 priorPointLane0 = _pointsLane0[0];
        Vector3 priorPointLane1 = _pointsLane1[0];
        for (int i = 1; i < _pointsLane0.Count; i++)
        {
            Vector3 nextPointLaneMinus1 = _pointsLaneMinus1[i];
            Vector3 nextPointLane0 = _pointsLane0[i];
            Vector3 nextPointLane1 = _pointsLane1[i];

            Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, nextPointLane0 - priorPointLane0);
            Vector3 euler = rotation.eulerAngles;
            euler.x -= 90;
            rotation = Quaternion.Euler(euler);

            GameObject instantiated = Instantiate(segmentPrefab);
            instantiated.transform.position = (nextPointLane0 + priorPointLane0) / 2 + Vector3.up * Random.Range(-.0001f, .0001f);

            instantiated.transform.rotation = rotation;


            Vector3 localScale = instantiated.transform.localScale;

            float length = Mathf.Max((priorPointLane0 - nextPointLane0).magnitude, (priorPointLaneMinus1 - nextPointLaneMinus1).magnitude, (priorPointLane1 - nextPointLane1).magnitude);
            localScale.y *= length / TRACK_SEGMENT_PREFAB_LENGTH;
            instantiated.transform.localScale = localScale;


            priorPointLaneMinus1 = nextPointLaneMinus1;
            priorPointLane0 = nextPointLane0;
            priorPointLane1 = nextPointLane1;
        }

    }
}