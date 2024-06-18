using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerMovementSettings))]
[ExecuteInEditMode]
public class PlayerMovementSettingsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        PlayerMovementSettings settings = target as PlayerMovementSettings;

        GUI.enabled = false;
        EditorGUILayout.FloatField("Gravity Acceleration While Rising", settings.GravityAccelerationWhileRising);
        EditorGUILayout.FloatField("Gravity Acceleration While Falling", settings.GravityAccelerationWhileFalling);
        GUI.enabled = true;
    }
}
