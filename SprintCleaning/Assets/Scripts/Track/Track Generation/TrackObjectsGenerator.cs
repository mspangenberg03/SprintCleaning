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
    [SerializeField] private int _maxConsecutiveBeatsWithLaneChange = 1;
    [Header("Beat strengths filled from 1st to last.")]
    [SerializeField] private GarbageSpawningBeatStrength[] _beatStrengths;

    private DictionaryOfPoolsOfMonoBehaviour<Garbage> _pools;
    private List<(int, GameObject)> _selectedBeatsAndPrefabs = new();
    private List<(float time, Vector3 finalPosition, Vector3 initialPosition, Quaternion rotation, TrackPiece trackPiece, GameObject prefab)> _plannedSpawns = new(); // should probably use a priority queue for this, and make this data a struct or class
    private int _priorLane;
    private int _priorBeat = -1;
    private int _consecutiveBeatsWithLaneChange;
    private System.Comparison<(int, GameObject)> _comparisonDelegateInstance = (a, b) => a.Item1 - b.Item1;
    private Dictionary<GameObject, float> _prefabToGravity = new();

    //public static TrackPiece TrackPieceGeneratingObjectsOn { get; private set; }

    public void Initialize(Transform poolFolder, Transform outOfPoolFolder)
    {
        _pools = new DictionaryOfPoolsOfMonoBehaviour<Garbage>(poolFolder, outOfPoolFolder);
        foreach (GarbageSpawningBeatStrength x in _beatStrengths)
        {
            foreach (GameObject prefab in x.GarbagePrefabs)
            {
                _pools.CheckAddPoolForPrefab(prefab);
                _prefabToGravity[prefab] = prefab.GetComponentInChildren<Garbage>().Gravity;
            }
        }
    }

    public void AddTrash(TrackPiece trackPiece, int numTrackPieces)
    {
        //TrackPieceGeneratingObjectsOn = trackPiece;

        int numTrash = Random.Range(_minGarbageOnTrackPiece, _maxGarbageOnTrackPiece + 1);
        if (numTrackPieces < 5)
            numTrash = 0; // so the player doesn't immediately encounter trash.

        if (!DevHelper.Instance.TrashCollectionTimingInfo.CheckTrashCollectionConsistentIntervals)
            GenerateTrashAtSomeBeats(numTrash, trackPiece);
        else
            GenerateTrashAtEveryPosition(trackPiece);

        
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
                CreateGarbage(prefab, finalPosition, rotation, trackPieceFromEarlier, true, initialPosition);
            }
        }
    }

    private void GenerateTrashAtSomeBeats(int numTrash, TrackPiece trackPiece)
    {
        foreach (GarbageSpawningBeatStrength x in _beatStrengths)
            x.StartNextTrackPiece();

        SelectBeatsAndPrefabs(numTrash);

        for (int i = 0; i < _selectedBeatsAndPrefabs.Count; i++)
        {
            (int beat, GameObject prefab) = _selectedBeatsAndPrefabs[i];
            int lane = SelectNextLane(beat);

            float distanceAlongMidline = beat * TrackPiece.TRACK_PIECE_LENGTH / 16;
            CalcPositionAndRotationForObjectOnTrack(trackPiece, distanceAlongMidline, lane, out Vector3 finalPosition, out Quaternion rotation);

            if (Random.value < _oddsSpawnImmediately)
            {
                CreateGarbage(prefab, finalPosition, rotation, trackPiece, false, Vector3.negativeInfinity);
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
                    CreateGarbage(prefab, finalPosition, rotation, trackPiece, false, Vector3.negativeInfinity);
                else
                {
                    float throwTime = Garbage.FallTime(initialPosition, finalPosition, _prefabToGravity[prefab]);

                    float timeUntilReachBeat = distanceToReachBeat / PlayerMovement.Settings.BaseForwardsSpeed;
                    float spawnDelay = timeUntilReachBeat - warningTime - throwTime;
                    if (spawnDelay <= 0)
                        CreateGarbage(prefab, finalPosition, rotation, trackPiece, false, Vector3.negativeInfinity);
                    else
                        _plannedSpawns.Insert(0, (Time.fixedTime + spawnDelay, finalPosition, initialPosition, rotation, trackPiece, prefab));
                }
            }
        }
    }

    private void CreateGarbage(GameObject prefab, Vector3 finalPosition, Quaternion rotation, TrackPiece trackPiece, bool thrown, Vector3 throwFrom)
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

    private void SelectBeatsAndPrefabs(int numTrash)
    {
        _selectedBeatsAndPrefabs.Clear();
        for (int i = 0; i < numTrash; i++)
        {
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
            _selectedBeatsAndPrefabs.Add((beat, prefab));
        }
        _selectedBeatsAndPrefabs.Sort(_comparisonDelegateInstance);
    }

    private int SelectNextLane(int beat)
    {
        bool isConsecutiveBeat = beat == _priorBeat + 1 || (beat == 0 && _priorBeat == 15);
        isConsecutiveBeat &= _priorBeat != -1;

        // Select the lane.
        int lane;
        if (isConsecutiveBeat)
        {
            if (_consecutiveBeatsWithLaneChange >= _maxConsecutiveBeatsWithLaneChange)
            {
                // Don't require the player to make too many consecutive lane changes.
                lane = _priorLane;
            }
            else
            {
                // Don't require the player to make two lane changes between consecutive beats.
                do
                {
                    lane = Random.Range(-1, 2);
                } while (System.Math.Abs(lane - _priorLane) > 1);
            }
        }
        else
        {
            _consecutiveBeatsWithLaneChange = 0;
            lane = Random.Range(-1, 2);
        }

        if (isConsecutiveBeat && lane != _priorLane)
            _consecutiveBeatsWithLaneChange++;
        else
            _consecutiveBeatsWithLaneChange = 0;
        _priorBeat = beat;
        _priorLane = lane;

        return lane;
    }



    private void GenerateTrashAtEveryPosition(TrackPiece trackPiece)
    {
        GameObject prefab = _beatStrengths[0].GarbagePrefabs[0];
        for (int i = 0; i < TrackPiece.TRACK_PIECE_LENGTH; i += TrackPiece.TRACK_PIECE_LENGTH / 16)
        {
            for (int j = -1; j <= 1; j++)
            {
                CalcPositionAndRotationForObjectOnTrack(trackPiece, i, j, out Vector3 position, out Quaternion rotation);
                Garbage newGarbage = _pools.Produce(prefab, position, rotation);
                newGarbage.OnTrackPiece = trackPiece;
#if UNITY_EDITOR
                if (trackPiece.GarbageOnThisTrackPiece.Contains(newGarbage))
                    throw new System.Exception("The garbage is already in the list.");
#endif
                trackPiece.GarbageOnThisTrackPiece.Add(newGarbage);
                
            }
        }
    }


    private void CalcPositionAndRotationForObjectOnTrack(TrackPiece trackPiece, float distanceAlongMidline, float lane
        , out Vector3 position, out Quaternion rotation)
    {
        float t = trackPiece.FindTForDistanceAlongMidline(distanceAlongMidline, 0f);

        trackPiece.StoreLane(0);
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
