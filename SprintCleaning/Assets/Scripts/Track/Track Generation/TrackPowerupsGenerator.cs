using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// a lot of this is copy & pasted from TrackObstaclesGenerator

[System.Serializable]
public class TrackPowerupsGenerator
{
    [SerializeField] private float _minTimeSeeObjectOnTrack = 1;
    [SerializeField] private float _maxTimeSeeObjectOnTrack = 2;
    [SerializeField] private float _trackObjectsYOffset = .5f;

    [SerializeField] private float _oddsSpawnImmediately = 1f;
    [SerializeField] private float _minPerTrackPiece = 0f;
    [SerializeField] private float _maxPerTrackPiece = 2f;
    [field: SerializeField] public GameObject[] PowerupPrefabs { get; private set; }

    private float _leftover;
    private List<int> _possibleBeats = new();
    private List<int> _possibleLanes = new();

    private TrackObjectsInstantiator _instantiator;

    public void Initialize(TrackObjectsInstantiator instantiator)
    {
        _instantiator = instantiator;
    }

    public void AddPowerupsToTrackPiece(TrackPiece trackPiece, bool spawnNone
        , List<(int beat, GameObject prefab, int lane)> selectedBeatsAndPrefabsAndLanes)
    {
        int numberToSpawn = NumberToSpawn();
        if (spawnNone)
            numberToSpawn = 0;
        GeneratePowerups(trackPiece, numberToSpawn, selectedBeatsAndPrefabsAndLanes);
    }


    private int NumberToSpawn()
    {
        float floatNumber = Random.Range(_minPerTrackPiece, _maxPerTrackPiece);

        _leftover += floatNumber;
        int result = (int)_leftover;
        _leftover -= result;
        result = System.Math.Min(result, Mathf.CeilToInt(_maxPerTrackPiece));
        return result;
    }

    private void GeneratePowerups(TrackPiece trackPiece, int num, List<(int beat, GameObject prefab, int lane)> selectedBeatsAndPrefabsAndLanes)
    {
        DecidePossibleBeats(selectedBeatsAndPrefabsAndLanes);

        int numSpawned = 0;
        int currentTrackLastObstacleBeat = -1;
        for (int i = 0; i < num && _possibleBeats.Count > 0; i++)
        {
            int beat = _possibleBeats.TakeRandomElement();

            DecidePossibleObstacleLanesForBeat(beat, selectedBeatsAndPrefabsAndLanes, out bool failed);
            if (failed)
            {
                // Retry with a different beat
                i--;
                continue;
            }

            GameObject prefab = PowerupPrefabs[Random.Range(0, PowerupPrefabs.Length)];
            int lane = _possibleLanes.RandomElement();

            _instantiator.SpawnOrPlanToThrowObject(prefab, beat, lane, trackPiece, _oddsSpawnImmediately, Random.Range(_minTimeSeeObjectOnTrack, _maxTimeSeeObjectOnTrack), _trackObjectsYOffset);

            numSpawned++;
            currentTrackLastObstacleBeat = System.Math.Max(currentTrackLastObstacleBeat, beat);
        }
    }

    private void DecidePossibleBeats(List<(int beat, GameObject prefab, int lane)> selectedBeatsAndPrefabsAndLanesForGarbage)
    {
        _possibleBeats.Clear();
        for (int i = 0; i < 16; i++)
            _possibleBeats.Add(i);
        for (int i = 0; i < selectedBeatsAndPrefabsAndLanesForGarbage.Count; i++)
            _possibleBeats.Remove(selectedBeatsAndPrefabsAndLanesForGarbage[i].beat);
    }

    private void DecidePossibleObstacleLanesForBeat(int beat, List<(int beat, GameObject prefab, int lane)> selectedBeatsAndPrefabsAndLanesForGarbage, out bool failed)
    {
        _possibleLanes.Clear();
        for (int j = -1; j <= 1; j++)
            _possibleLanes.Add(j);

        failed = false;
    }
}
