using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class ClearPlayerPrefs
{
    [UnityEditor.MenuItem("Custom/Clear PlayerPrefs")]
    public static void ClearPrefs()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("Cleared playerPrefs");
    }
}
