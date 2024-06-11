using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Will have an array of these in order from most to least strong
[System.Serializable]
public class GarbageSpawningBeatStrength
{
    [field: SerializeField] public int[] Beats { get; private set; }
    [field: SerializeField] public GameObject[] GarbagePrefabs { get; private set; }

    private List<int> _remainingBeats = new();

    public void StartNextTrackPiece()
    {
        _remainingBeats.Clear();
        foreach (int x in Beats)
        {
            _remainingBeats.Add(x);
        }
    }

    public void Next(out bool allFull, out int beatToSpawnAt, out GameObject prefab)
    {
        allFull = _remainingBeats.Count == 0;
        if (allFull)
        {
            beatToSpawnAt = -1;
            prefab = null;
            return;
        }

        int index = Random.Range(0, _remainingBeats.Count);
        beatToSpawnAt = _remainingBeats[index];
        _remainingBeats.RemoveAt(index);

        prefab = GarbagePrefabs[Random.Range(0, GarbagePrefabs.Length)];
    }
}
