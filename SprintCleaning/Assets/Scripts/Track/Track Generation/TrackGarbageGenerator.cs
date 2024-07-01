using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TrackGarbageGenerator
{
    [SerializeField] private int _maxConsecutiveBeatsWithLaneChange = 1;
    [SerializeField] private float _oddsForciblyChangeLane = .5f;
    [SerializeField] private float _trackObjectsYOffset = .5f;
    [SerializeField] private float _minTrackPiecesPerRest;
    [SerializeField] private float _maxTrackPiecesPerRest;

    [Header("Non-Rest Track Piece")]
    [SerializeField] private int _minGarbageOnTrackPiece = 4;
    [SerializeField] private int _maxGarbageOnTrackPiece = 16;
    [SerializeField] private int _numberOfGarbageCountsToChooseMinAmongst = 2;

    [SerializeField] private float _minTimeSeeObjectOnTrack = 1;
    [SerializeField] private float _maxTimeSeeObjectOnTrack = 2;
    [SerializeField] private float _oddsSpawnImmediately = .5f;

    [Header("Rest Track Piece")]
    [SerializeField] private int _minGarbageOnRestTrackPiece = 4;
    [SerializeField] private int _maxGarbageOnRestTrackPiece = 16;
    [SerializeField] private int _numberOfGarbageCountsToChooseMinAmongstOnRest = 2;

    [SerializeField] private float _minTimeSeeObjectOnRestTrack = 1;
    [SerializeField] private float _maxTimeSeeObjectOnRestTrack = 2;
    [SerializeField] private float _oddsSpawnImmediatelyOnRest = .5f;

    [field: Header("Beat Strengths (filled from 1st to last)")]
    [field: SerializeField] public GarbageSpawningBeatStrength[] BeatStrengths { get; private set; }

    private List<(int beat, GameObject prefab, int lane)> _selectedBeatsAndPrefabsAndLanesForGarbage = new();
    private TrackGarbageLaneDecider _laneDecider;

    private float _trackPiecesUntilRest;

    private System.Comparison<(int, GameObject, int)> _comparisonDelegateInstance = (a, b) => a.Item1 - b.Item1; // cache this in a field for garbage



    private TrackObjectsInstantiator _instantiator;

    public void Initialize(TrackObjectsInstantiator instantiator)
    {
        _instantiator = instantiator;
        _laneDecider = new TrackGarbageLaneDecider(_maxConsecutiveBeatsWithLaneChange, _oddsForciblyChangeLane);
    }


    public void AddGarbage(TrackPiece trackPiece, bool spawnNone
        , out List<(int beat, GameObject prefab, int lane)> selectedBeatsAndPrefabsAndLanesForGarbage)
    {
        _trackPiecesUntilRest--;
        bool rest = _trackPiecesUntilRest < 0;
        if (rest)
            _trackPiecesUntilRest += Random.Range(_minTrackPiecesPerRest, _maxTrackPiecesPerRest);

        int numTrash = NumberOfTrashToSpawn(rest);
        if (spawnNone)
            numTrash = 0;

        GenerateGarbage(trackPiece, numTrash, rest);

        selectedBeatsAndPrefabsAndLanesForGarbage = _selectedBeatsAndPrefabsAndLanesForGarbage;
    }

    private int NumberOfTrashToSpawn(bool rest)
    {
        int min = rest ? _minGarbageOnRestTrackPiece : _minGarbageOnTrackPiece;
        int max = rest ? _maxGarbageOnRestTrackPiece : _maxGarbageOnTrackPiece;
        int attempts = rest ? _numberOfGarbageCountsToChooseMinAmongstOnRest : _numberOfGarbageCountsToChooseMinAmongst;
        int result = int.MaxValue;
        for (int i = 0; i < attempts; i++)
            result = System.Math.Min(result, Random.Range(min, max + 1));
        return result;
    }

    private void GenerateGarbage(TrackPiece trackPiece, int numTrash, bool rest)
    {
        foreach (GarbageSpawningBeatStrength x in BeatStrengths)
            x.StartNextTrackPiece();

        SelectBeatsAndPrefabsForGarbage(numTrash);

        for (int i = 0; i < _selectedBeatsAndPrefabsAndLanesForGarbage.Count; i++)
        {
            (int beat, GameObject prefab, _) = _selectedBeatsAndPrefabsAndLanesForGarbage[i];
            int lane = _laneDecider.SelectGarbageLane(beat);

            _selectedBeatsAndPrefabsAndLanesForGarbage[i] = (beat, prefab, lane);
            float delay = Random.Range(_minTimeSeeObjectOnTrack, _maxTimeSeeObjectOnTrack);
            if (rest)
                delay = Random.Range(_minTimeSeeObjectOnRestTrack, _maxTimeSeeObjectOnRestTrack);
            float oddsSpawnImmediately = rest ? _oddsSpawnImmediatelyOnRest : _oddsSpawnImmediately;
            _instantiator.SpawnOrPlanToThrowObject(prefab, beat, lane, trackPiece, oddsSpawnImmediately, delay, _trackObjectsYOffset);
        }
    }

    private void SelectBeatsAndPrefabsForGarbage(int numTrash)
    {
        _selectedBeatsAndPrefabsAndLanesForGarbage.Clear();
        for (int i = 0; i < numTrash; i++)
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
