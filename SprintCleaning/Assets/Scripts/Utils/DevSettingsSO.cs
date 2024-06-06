using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DevSettings", menuName = "DevSettings")]
public class DevSettingsSO : ScriptableObject
{
    [field: SerializeField] public bool ReproduceSavedRNGAndInputs { get; private set; }
    [field: SerializeField] public bool CheckTrashCollectionConsistentIntervals { get; private set; }
    [Tooltip("How many fixed updates occur between trash collection when CheckTrashCollectionConsistentIntervals is true.\n" +
        "It depends on your monitor's refresh rate so enter play mode and check the numbers which get spammed in the console.")]
    [field: SerializeField] public int[] ExpectedFixedUpdatesBetweenTrashCollection = new int[] { 28, 29 };
}
