using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TrackObjectsGenerator
{
    [SerializeField] private float _trackObjectsYOffset = 1.5f;
    [SerializeField] private int _minGarbageOnTrackPiece = 4;
    [SerializeField] private int _maxGarbageOnTrackPiece = 16;
    [Header("Beat strengths filled from 1st to last.")]
    [SerializeField] private GarbageSpawningBeatStrength[] _beatStrengths;

    private Transform _instantiatedGameObjectsParent;
    private int _laneChosenAtLastBeatOfPriorTrackPiece = -2;
    private int[] _laneChosenAtEachBeat = new int[16];
    private List<List<GameObject>> _spawnedObjects = new();
    private ObjectPool<List<GameObject>> _poolOfListsOfGameObjects = new();

    public void Initialize(Transform instantiatedGameObjectsParent)
    {
        _instantiatedGameObjectsParent = instantiatedGameObjectsParent;
    }

    public void AddTrash(TrackPiece trackPiece, int numTrackPieces)
    {
        // Create or reuse a list to store the objects on this trackPiece
        List<GameObject> gameObjectsOnNewTrackPiece = _poolOfListsOfGameObjects.ProduceObject();
        _spawnedObjects.Add(gameObjectsOnNewTrackPiece);

        int numTrash = Random.Range(_minGarbageOnTrackPiece, _maxGarbageOnTrackPiece + 1);
        if (numTrackPieces < 3)
            numTrash = 0; // so the player doesn't immediately encounter trash.

        if (!DevHelper.Instance.TrashCollectionTimingInfo.CheckTrashCollectionConsistentIntervals)
            GenerateTrash(numTrash, trackPiece, gameObjectsOnNewTrackPiece);
        else
            GenerateTrashAtEveryPosition(trackPiece, gameObjectsOnNewTrackPiece);
    }

    private void GenerateTrash(int numTrash, TrackPiece trackPiece, List<GameObject> gameObjectsOnNewTrackPiece)
    {
        _laneChosenAtLastBeatOfPriorTrackPiece = _laneChosenAtEachBeat[^1];
        for (int i = 0; i < _laneChosenAtEachBeat.Length; i++)
            _laneChosenAtEachBeat[i] = -2;
        foreach (GarbageSpawningBeatStrength g in _beatStrengths)
            g.StartNextTrackPiece();

        for (int i = 0; i < numTrash; i++)
        {
            bool success = false;
            for (int j = 0; j < _beatStrengths.Length; j++)
            {
                GarbageSpawningBeatStrength beatStrength = _beatStrengths[j];
                beatStrength.Next(out bool allFull, out int beatToSpawnAt, out GameObject prefab);
                if (allFull)
                    continue; // this inner for loop is just to find the first beatStrength which isn't full

                int lane;
                bool invalid;
                do
                {
                    lane = Random.Range(-1, 2);
                    invalid = LaneIsInvalid(beatToSpawnAt, lane);
                } while (invalid);

                _laneChosenAtEachBeat[beatToSpawnAt] = lane;

                float distanceAlongMidline = beatToSpawnAt * TrackPiece.TRACK_PIECE_LENGTH / 16;
                ChooseRandomPositionAndRotationForObjectOnTrack(trackPiece, distanceAlongMidline, lane, out Vector3 position, out Quaternion rotation);
                GameObject instantiated = Object.Instantiate(prefab, position, rotation, _instantiatedGameObjectsParent);
                gameObjectsOnNewTrackPiece.Add(instantiated);
                success = true;
                break;
            }
            if (!success)
                throw new System.Exception("Failed to find a position to spawn. This is a bug or the inspector settings have more trash spawn than the number of beats.");
        }
    }

    private bool LaneIsInvalid(int beat, int laneAtBeat)
    {
        if (DistanceFromLaneAtBeat(beat - 1, laneAtBeat) > 1)
            return true;
        if (DistanceFromLaneAtBeat(beat + 1, laneAtBeat) > 1)
            return true;
        return false;

        int DistanceFromLaneAtBeat(int beat, int lane)
        {
            if (beat == -1)
                return System.Math.Abs(lane - _laneChosenAtLastBeatOfPriorTrackPiece);
            if (beat == _laneChosenAtEachBeat.Length)
                return 0;
            if (_laneChosenAtEachBeat[beat] == -2) // the lane at this beat hasn't been chosen
                return 0;
            return System.Math.Abs(lane - _laneChosenAtEachBeat[beat]);
        }
    }

    private void GenerateTrashAtEveryPosition(TrackPiece trackPiece, List<GameObject> gameObjectsOnNewTrackPiece)
    {
        GameObject prefab = _beatStrengths[0].GarbagePrefabs[0];
        for (int i = 0; i < TrackPiece.TRACK_PIECE_LENGTH; i += TrackPiece.TRACK_PIECE_LENGTH / 16)
        {
            for (int j = -1; j <= 1; j++)
            {
                ChooseRandomPositionAndRotationForObjectOnTrack(trackPiece, i, j, out Vector3 position, out Quaternion rotation);
                GameObject instantiated = Object.Instantiate(prefab, position, rotation, _instantiatedGameObjectsParent);
                gameObjectsOnNewTrackPiece.Add(instantiated);
            }
        }
    }


    private void ChooseRandomPositionAndRotationForObjectOnTrack(TrackPiece trackPiece, float distanceAlongMidline, float lane
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
