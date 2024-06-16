using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TrackObjectsGenerator
{
    [SerializeField] private float _trackObjectsYOffset = 1.5f;
    [SerializeField] private int _minGarbageOnTrackPiece = 4;
    [SerializeField] private int _maxGarbageOnTrackPiece = 16;
    [SerializeField] private int _maxConsecutiveBeatsWithLaneChange = 1;
    [Header("Beat strengths filled from 1st to last.")]
    [SerializeField] private GarbageSpawningBeatStrength[] _beatStrengths;

    private DictionaryOfPoolsOfMonoBehaviour<Garbage> _pools;
    private List<(int, GameObject)> _selectedBeatsAndPrefabs = new();
    private int _priorLane;
    private int _priorBeat = -1;
    private int _consecutiveBeatsWithLaneChange;
    private System.Comparison<(int, GameObject)> _comparisonDelegateInstance = (a, b) => a.Item1 - b.Item1;

    public static TrackPiece TrackPieceGeneratingObjectsOn { get; private set; }

    public void Initialize(Transform instantiatedGameObjectsParent)
    {
        _pools = new DictionaryOfPoolsOfMonoBehaviour<Garbage>(instantiatedGameObjectsParent);
        foreach (GarbageSpawningBeatStrength x in _beatStrengths)
        {
            foreach (GameObject prefab in x.GarbagePrefabs)
                _pools.CheckAddPoolForPrefab(prefab);
        }
    }

    public void AddTrash(TrackPiece trackPiece, int numTrackPieces)
    {
        TrackPieceGeneratingObjectsOn = trackPiece;

        int numTrash = Random.Range(_minGarbageOnTrackPiece, _maxGarbageOnTrackPiece + 1);
        if (numTrackPieces < 3)
            numTrash = 0; // so the player doesn't immediately encounter trash.

        if (!DevHelper.Instance.TrashCollectionTimingInfo.CheckTrashCollectionConsistentIntervals)
            GenerateTrashAtSomeBeats(numTrash, trackPiece);
        else
            GenerateTrashAtEveryPosition(trackPiece);
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
            CalcPositionAndRotationForObjectOnTrack(trackPiece, distanceAlongMidline, lane, out Vector3 position, out Quaternion rotation);

            Garbage newGarbage = _pools.Produce(prefab, position, rotation);
            trackPiece.GarbageOnThisTrackPiece.Add(newGarbage);
        }
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
