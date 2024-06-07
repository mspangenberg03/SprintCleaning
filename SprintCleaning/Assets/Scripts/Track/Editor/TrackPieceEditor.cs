using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TrackPiece))]
[ExecuteInEditMode]
public class TrackPieceEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        TrackPiece trackPiece = target as TrackPiece;

        GUI.enabled = false;
        double length = trackPiece.ApproximateMidlineLengthForEditor(100 * 1000);
        EditorGUILayout.DoubleField("Length", length);
        EditorGUILayout.DoubleField("Multiply Length By", TrackPiece.TRACK_PIECE_LENGTH / length);
        GUI.enabled = true;
    }
}
