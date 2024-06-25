using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TrackObstaclesGenerator
{
    [SerializeField] private float _minTimeSeeObjectOnTrack = 1;
    [SerializeField] private float _maxTimeSeeObjectOnTrack = 2;
    [SerializeField] private float _trackObjectsYOffset = .5f;

    [SerializeField] private float _oddsSpawnObstacleImmediately = 1f;
    [SerializeField] private float _minObstaclesPerTrackPiece = 0f;
    [SerializeField] private float _maxObstaclesPerTrackPiece = 2f;
    [SerializeField] private int _maxObstaclesCarriedOnWhenFailToSpawn = 2;
    [SerializeField] private int _numberOfObstacleCountsToChooseMinAmongst = 2;
    [SerializeField] private int _minBeatsBetweenObstacles = 3;
    [field: SerializeField] public GameObject[] ObstaclePrefabs { get; private set; }
    [field: SerializeField] public GameObject[] WideObstaclePrefabs { get; private set; }

    private float _obstaclesLeftover;
    private List<int> _possibleObstacleBeats = new();
    private List<int> _possibleObstacleLanes = new();
    private int _priorTrackPieceLastObstacleBeat = -1;

    [System.NonSerialized]
    private TrackObjectsGenerator _objectsGenerator;

    public void Initialize(TrackObjectsGenerator objectsGenerator)
    {
        _objectsGenerator = objectsGenerator;
    }

    public void AddObstaclesToTrackPiece(TrackPiece trackPiece, bool spawnNone
        , List<(int beat, GameObject prefab, int lane)> selectedBeatsAndPrefabsAndLanesForGarbage)
    {
        int numberToSpawn = NumberOfObstaclesToSpawn();
        if (spawnNone)
            numberToSpawn = 0;
        GenerateObstacles(trackPiece, numberToSpawn, selectedBeatsAndPrefabsAndLanesForGarbage);
    }


    private int NumberOfObstaclesToSpawn()
    {
        float floatNumberOfObstacles = float.PositiveInfinity;
        for (int i = 0; i < _numberOfObstacleCountsToChooseMinAmongst; i++)
            floatNumberOfObstacles = Mathf.Min(floatNumberOfObstacles, Random.Range(_minObstaclesPerTrackPiece, _maxObstaclesPerTrackPiece));

        _obstaclesLeftover += floatNumberOfObstacles;
        int result = (int)_obstaclesLeftover;
        _obstaclesLeftover -= result;
        result = System.Math.Min(result, Mathf.CeilToInt(_maxObstaclesPerTrackPiece));
        return result;
    }

    private void GenerateObstacles(TrackPiece trackPiece, int numObstacles, List<(int beat, GameObject prefab, int lane)> selectedBeatsAndPrefabsAndLanesForGarbage)
    {
        DecidePossibleObstacleBeats(selectedBeatsAndPrefabsAndLanesForGarbage);

        int numObstaclesSpawned = 0;
        int currentTrackLastObstacleBeat = -1;
        for (int i = 0; i < numObstacles && _possibleObstacleBeats.Count > 0; i++)
        {
            int beat = _possibleObstacleBeats.TakeRandomElement();

            DecidePossibleObstacleLanesForBeat(beat, selectedBeatsAndPrefabsAndLanesForGarbage, out bool failed);
            if (failed)
            {
                // Retry with a different beat
                i--;
                continue;
            }

            // Choose a random obstacle and random possible lane.
            // 3-wide obstacles only spawn if all 3 lanes are possible, and they spawn at the middle lane.
            int index = Random.Range(0, ObstaclePrefabs.Length + (_possibleObstacleLanes.Count == 3 ? WideObstaclePrefabs.Length : 0));
            GameObject prefab = index < ObstaclePrefabs.Length ? ObstaclePrefabs[index] : WideObstaclePrefabs[index - ObstaclePrefabs.Length];
            int lane = index < ObstaclePrefabs.Length ? _possibleObstacleLanes.RandomElement() : 0;

            _objectsGenerator.SpawnOrPlanToThrowObject(prefab, beat, lane, trackPiece, _oddsSpawnObstacleImmediately, Random.Range(_minTimeSeeObjectOnTrack, _maxTimeSeeObjectOnTrack), _trackObjectsYOffset);

            numObstaclesSpawned++;
            currentTrackLastObstacleBeat = System.Math.Max(currentTrackLastObstacleBeat, beat);

            // Remove possible beats around the selected beat
            for (int j = System.Math.Max(0, beat - _minBeatsBetweenObstacles); j <= System.Math.Min(15, beat + _minBeatsBetweenObstacles); j++)
                _possibleObstacleBeats.Remove(j);
        }
        _priorTrackPieceLastObstacleBeat = currentTrackLastObstacleBeat;

        // If not it didn't spawn enough obstacles, spawn a bit more next time
        int numObstaclesFailedToSpawn = numObstacles - numObstaclesSpawned;
        _obstaclesLeftover += System.Math.Min(numObstaclesFailedToSpawn, _maxObstaclesCarriedOnWhenFailToSpawn);
    }

    private void DecidePossibleObstacleBeats(List<(int beat, GameObject prefab, int lane)> selectedBeatsAndPrefabsAndLanesForGarbage)
    {
        _possibleObstacleBeats.Clear();
        for (int i = 0; i < 16; i++)
            _possibleObstacleBeats.Add(i);
        for (int i = 0; i < selectedBeatsAndPrefabsAndLanesForGarbage.Count; i++)
            _possibleObstacleBeats.Remove(selectedBeatsAndPrefabsAndLanesForGarbage[i].beat);
        if (_priorTrackPieceLastObstacleBeat != -1)
        {
            for (int i = _priorTrackPieceLastObstacleBeat; i <= _priorTrackPieceLastObstacleBeat + _minBeatsBetweenObstacles; i++)
            {
                int toRemove = (i + 32) % 16;
                if (toRemove >= _priorTrackPieceLastObstacleBeat)
                    continue;
                _possibleObstacleBeats.Remove(toRemove);
            }
        }
    }

    private void DecidePossibleObstacleLanesForBeat(int beat, List<(int beat, GameObject prefab, int lane)> selectedBeatsAndPrefabsAndLanesForGarbage, out bool failed)
    {
        _possibleObstacleLanes.Clear();
        for (int j = -1; j <= 1; j++)
            _possibleObstacleLanes.Add(j);

        for (int j = 0; j < selectedBeatsAndPrefabsAndLanesForGarbage.Count; j++)
        {
            (int beatOfTrash, _, int laneOfTrash) = selectedBeatsAndPrefabsAndLanesForGarbage[j];
            if (System.Math.Abs(beatOfTrash - beat) < 2)
                _possibleObstacleLanes.Remove(laneOfTrash);
        }

        // If there's only 1 lane, that lane would be on the path between the two adjacent pieces of trash.
        // The 2nd part of the || is for a case where it happens.
        failed = _possibleObstacleLanes.Count < 2 || (beat == 15 && _possibleObstacleLanes.Count < 3);
    }
}
