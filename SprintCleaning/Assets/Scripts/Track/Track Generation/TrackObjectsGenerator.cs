using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TrackObjectsGenerator
{
    [SerializeField] private float _minTimeSeeObjectOnTrack = 5;
    [SerializeField] private float _maxTimeSeeObjectOnTrack = 10;
    [SerializeField] private float _oddsSpawnImmediately = .5f;
    [SerializeField] private float _trackObjectsYOffset = 1.5f;
    [SerializeField] private int _minGarbageOnTrackPiece = 4;
    [SerializeField] private int _maxGarbageOnTrackPiece = 16;
    [SerializeField] private int _numberOfGarbageCountsToChooseMinAmongst = 2;
    [SerializeField] private int _maxConsecutiveBeatsWithLaneChange = 1;
    [Header("Beat Strengths (filled from 1st to last)")]
    [SerializeField] private GarbageSpawningBeatStrength[] _beatStrengths;


    private DictionaryOfPoolsOfMonoBehaviour<Garbage> _pools;

    private List<(int beat, GameObject prefab, int lane)> _selectedBeatsAndPrefabsAndLanesForGarbage = new();
    private List<(float time, Vector3 finalPosition, Vector3 initialPosition, Quaternion rotation, TrackPiece trackPiece, GameObject prefab)> _plannedSpawns = new();
     //^ should probably use a priority queue for this, and make this data a struct or class

    private TrackGarbageLaneDecider _laneDecider;
    private System.Comparison<(int, GameObject, int)> _comparisonDelegateInstance = (a, b) => a.Item1 - b.Item1;
    private Dictionary<GameObject, float> _prefabToGravity = new();


    private TrackObstaclesGenerator _obstaclesGenerator;

    public void Initialize(Transform poolFolder, Transform outOfPoolFolder, TrackObstaclesGenerator obstaclesGenerator)
    {
        _obstaclesGenerator= obstaclesGenerator;
        _laneDecider = new TrackGarbageLaneDecider(_maxConsecutiveBeatsWithLaneChange);
        _pools = new DictionaryOfPoolsOfMonoBehaviour<Garbage>(poolFolder, outOfPoolFolder);


        foreach (GarbageSpawningBeatStrength x in _beatStrengths)
        {
            foreach (GameObject prefab in x.GarbagePrefabs)
                InitializeForPrefab(prefab);
        }
        foreach (GameObject prefab in obstaclesGenerator.ObstaclePrefabs)
            InitializeForPrefab(prefab);
        foreach (GameObject prefab in obstaclesGenerator.WideObstaclePrefabs)
            InitializeForPrefab(prefab);

        void InitializeForPrefab(GameObject prefab)
        {
            _pools.CheckAddPoolForPrefab(prefab);
            _prefabToGravity[prefab] = prefab.GetComponentInChildren<Garbage>().Gravity;
        }

        obstaclesGenerator.Initialize(this);
    }

    public void AddGarbageAndObstacles(TrackPiece trackPiece, int numTrackPieces)
    {
        bool spawnNone = numTrackPieces < 4; // To prevent immediately encountering trash when the run starts.


        int numTrash = NumberOfTrashToSpawn();
        if (spawnNone)
            numTrash = 0;

        GenerateGarbage(trackPiece, numTrash);
        _obstaclesGenerator.AddObstaclesToTrackPiece(trackPiece, spawnNone, _selectedBeatsAndPrefabsAndLanesForGarbage);
    }

    private int NumberOfTrashToSpawn()
    {
        int result = int.MaxValue;
        for (int i = 0; i < _numberOfGarbageCountsToChooseMinAmongst; i++)
            result = System.Math.Min(result, Random.Range(_minGarbageOnTrackPiece, _maxGarbageOnTrackPiece + 1));
        return result;
    }

    public void AfterPlayerMovementFixedUpdate()
    {
        CheckSpawnPlannedTrash();

        for (int i = Garbage.ThrownGarbage.Count - 1; i >= 0; i--)
            Garbage.ThrownGarbage[i].MoveWhileBeingThrown();
    }

    private void CheckSpawnPlannedTrash()
    {
        for (int i = _plannedSpawns.Count - 1; i >= 0; i--)
        {
            if (Time.fixedTime >= _plannedSpawns[i].time)
            {
                (_, Vector3 finalPosition, Vector3 initialPosition, Quaternion rotation, TrackPiece trackPieceFromEarlier, GameObject prefab) = _plannedSpawns[i];
                _plannedSpawns.RemoveAt(i);
                Spawn(prefab, finalPosition, rotation, trackPieceFromEarlier, true, initialPosition);
            }
        }
    }

    private void GenerateGarbage(TrackPiece trackPiece, int numTrash)
    {
        foreach (GarbageSpawningBeatStrength x in _beatStrengths)
            x.StartNextTrackPiece();

        SelectBeatsAndPrefabsForGarbage(numTrash);

        for (int i = 0; i < _selectedBeatsAndPrefabsAndLanesForGarbage.Count; i++)
        {
            (int beat, GameObject prefab, _) = _selectedBeatsAndPrefabsAndLanesForGarbage[i];
            int lane = _laneDecider.SelectGarbageLane(beat);

            _selectedBeatsAndPrefabsAndLanesForGarbage[i] = (beat, prefab, lane);
            SpawnOrPlanToThrowObject(prefab, beat, lane, trackPiece, _oddsSpawnImmediately);
        }
    }

    public void SpawnOrPlanToThrowObject(GameObject prefab, int beat, int lane, TrackPiece trackPiece, float oddsSpawnImmediately)
    {
        float distanceAlongMidline = beat * TrackPiece.TRACK_PIECE_LENGTH / 16;
        CalcPositionAndRotationForObjectOnTrack(trackPiece, distanceAlongMidline, lane, out Vector3 finalPosition, out Quaternion rotation);

        if (Random.value <= oddsSpawnImmediately)
        {
            Spawn(prefab, finalPosition, rotation, trackPiece, false, Vector3.negativeInfinity);
        }
        else
        {
            float warningTime = Random.Range(_minTimeSeeObjectOnTrack, _maxTimeSeeObjectOnTrack);

            // For distance, don't need to deal with what fraction of a track piece the player has traversed, because this code runs when the player is at the border between two track pieces.
            // Also can just consider the length of the track midline because the player effectively travels along that (different speed for different lanes).
            float distanceToReachBeat = ((float)beat) / 16 * TrackPiece.TRACK_PIECE_LENGTH;
            TrackPiece x = trackPiece.Prior;
            while (x != null && x != TrackGenerator.Instance.TrackPieces[0])
            {
                distanceToReachBeat += TrackPiece.TRACK_PIECE_LENGTH;
                x = x.Prior;
            }

            Vector3 initialPosition = Building.GetInitialPositionForThrownTrash(finalPosition);

            bool wouldNeedToFallUpwards = finalPosition.y > initialPosition.y - .1f; // the subtraction is to avoid ridiculously fast horizontal throwing speeds
            if (wouldNeedToFallUpwards)
                Spawn(prefab, finalPosition, rotation, trackPiece, false, Vector3.negativeInfinity);
            else
            {
                float throwTime = Garbage.FallTime(initialPosition, finalPosition, _prefabToGravity[prefab]);

                float timeUntilReachBeat = distanceToReachBeat / PlayerMovement.Settings.BaseForwardsSpeed;
                float spawnDelay = timeUntilReachBeat - warningTime - throwTime;
                if (spawnDelay <= 0)
                    Spawn(prefab, finalPosition, rotation, trackPiece, false, Vector3.negativeInfinity);
                else
                    _plannedSpawns.Insert(0, (Time.fixedTime + spawnDelay, finalPosition, initialPosition, rotation, trackPiece, prefab));
            }
        }
    }

    private void Spawn(GameObject prefab, Vector3 finalPosition, Quaternion rotation, TrackPiece trackPiece, bool thrown, Vector3 throwFrom)
    {
        Vector3 initialPosition = thrown ? throwFrom : finalPosition;
        Garbage newGarbage = _pools.Produce(prefab, initialPosition, rotation);

        if (thrown)
            newGarbage.SetTrajectoryFromCurrentPosition(finalPosition);

        newGarbage.OnTrackPiece = trackPiece;
#if UNITY_EDITOR
        if (newGarbage.InPool())
            throw new System.Exception("The garbage is still in the pool.");
        if (trackPiece.GarbageOnThisTrackPiece.Contains(newGarbage))
            throw new System.Exception("The garbage is already in the list.");
#endif
        trackPiece.GarbageOnThisTrackPiece.Add(newGarbage);
    }

    private void SelectBeatsAndPrefabsForGarbage(int numTrash)
    {
        _selectedBeatsAndPrefabsAndLanesForGarbage.Clear();
        for (int i = 0; i < numTrash; i++)
        {
            // Find the 1st beat strength which isn't full.
            GarbageSpawningBeatStrength beatStrength = null;
            for (int j = 0; j < _beatStrengths.Length; j++)
            {
                if (!_beatStrengths[j].IsFull())
                {
                    beatStrength = _beatStrengths[j];
                    break;
                }
            }

            beatStrength.Next(out int beat, out GameObject prefab);
            _selectedBeatsAndPrefabsAndLanesForGarbage.Add((beat, prefab, -2));
        }
        _selectedBeatsAndPrefabsAndLanesForGarbage.Sort(_comparisonDelegateInstance);
    }

    private void CalcPositionAndRotationForObjectOnTrack(TrackPiece trackPiece, float distanceAlongMidline, float lane
        , out Vector3 position, out Quaternion rotation)
    {
        trackPiece.StoreLane(0);
        float t = trackPiece.FindTForDistanceAlongStoredLane(distanceAlongMidline, 0f);

        Vector3 midlinePosition = trackPiece.BezierCurve(t) + Vector3.up * _trackObjectsYOffset;

        Vector3 approximatedPositionOnMidline = trackPiece.BezierCurve(t);
        trackPiece.StoreLane(lane);
        Vector3 approximatedPositionAtLanePosition = trackPiece.BezierCurve(t);
        Vector3 offsetForLanePosition = approximatedPositionAtLanePosition - approximatedPositionOnMidline;
        position = midlinePosition + offsetForLanePosition;


        Vector3 direction = trackPiece.BezierCurveDerivative(t);
        Vector3 directionOnPlane = new Vector3(direction.x, 0, direction.z);
        float directionAngle = Quaternion.FromToRotation(Vector3.forward, directionOnPlane).eulerAngles.y;
        rotation = Quaternion.Euler(0f, directionAngle, 0f);
    }

}
