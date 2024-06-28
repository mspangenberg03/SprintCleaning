using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TrackGarbageGenerator
{
    [SerializeField] private float _minTimeSeeObjectOnTrack = 1;
    [SerializeField] private float _maxTimeSeeObjectOnTrack = 2;
    [SerializeField] private float _trackObjectsYOffset = .5f;

    [SerializeField] private float _oddsSpawnImmediately = .5f;
    [SerializeField] private int _minGarbageOnTrackPiece = 4;
    [SerializeField] private int _maxGarbageOnTrackPiece = 16;
    [SerializeField] private int _numberOfGarbageCountsToChooseMinAmongst = 2;
    [SerializeField] private int _maxConsecutiveBeatsWithLaneChange = 1;
    [field: Header("Beat Strengths (filled from 1st to last)")]
    [field: SerializeField] public GarbageSpawningBeatStrength[] BeatStrengths { get; private set; }

    private List<(int beat, GameObject prefab, int lane)> _selectedBeatsAndPrefabsAndLanesForGarbage = new();
    private TrackGarbageLaneDecider _laneDecider;

    private System.Comparison<(int, GameObject, int)> _comparisonDelegateInstance = (a, b) => a.Item1 - b.Item1; // cache this in a field for garbage



    private TrackObjectsInstantiator _instantiator;

    public void Initialize(TrackObjectsInstantiator instantiator)
    {
        _instantiator = instantiator;
        _laneDecider = new TrackGarbageLaneDecider(_maxConsecutiveBeatsWithLaneChange);
    }


    public void AddGarbage(TrackPiece trackPiece, bool spawnNone
        , out List<(int beat, GameObject prefab, int lane)> selectedBeatsAndPrefabsAndLanesForGarbage)
    {
        int numTrash = NumberOfTrashToSpawn();
        if (spawnNone)
            numTrash = 0;
        GenerateGarbage(trackPiece, numTrash);

        selectedBeatsAndPrefabsAndLanesForGarbage = _selectedBeatsAndPrefabsAndLanesForGarbage;
    }

    private int NumberOfTrashToSpawn()
    {
        int result = int.MaxValue;
        for (int i = 0; i < _numberOfGarbageCountsToChooseMinAmongst; i++)
            result = System.Math.Min(result, Random.Range(_minGarbageOnTrackPiece, _maxGarbageOnTrackPiece + 1));
        return result;
    }

    private void GenerateGarbage(TrackPiece trackPiece, int numTrash)
    {
        foreach (GarbageSpawningBeatStrength x in BeatStrengths)
            x.StartNextTrackPiece();

        SelectBeatsAndPrefabsForGarbage(numTrash);

        for (int i = 0; i < _selectedBeatsAndPrefabsAndLanesForGarbage.Count; i++)
        {
            (int beat, GameObject prefab, _) = _selectedBeatsAndPrefabsAndLanesForGarbage[i];
            int lane = 0;// _laneDecider.SelectGarbageLane(beat);

            _selectedBeatsAndPrefabsAndLanesForGarbage[i] = (beat, prefab, lane);
            _instantiator.SpawnOrPlanToThrowObject(prefab, beat, lane, trackPiece, _oddsSpawnImmediately, Random.Range(_minTimeSeeObjectOnTrack, _maxTimeSeeObjectOnTrack), _trackObjectsYOffset);
        }
    }

    private void SelectBeatsAndPrefabsForGarbage(int numTrash)
    {
        _selectedBeatsAndPrefabsAndLanesForGarbage.Clear();
        for (int i = 0; i < 16; i++)
        {
            // Find the 1st beat strength which isn't full.
            GarbageSpawningBeatStrength beatStrength = null;
            for (int j = 0; j < BeatStrengths.Length; j++)
            {
                if (!BeatStrengths[j].IsFull())
                {
                    beatStrength = BeatStrengths[j];
                    break;
                }
            }

            beatStrength.Next(out int beat, out GameObject prefab);
            _selectedBeatsAndPrefabsAndLanesForGarbage.Add((beat, prefab, -2));
        }
        _selectedBeatsAndPrefabsAndLanesForGarbage.Sort(_comparisonDelegateInstance);
    }

}
