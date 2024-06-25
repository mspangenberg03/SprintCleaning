using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackObjectsInstantiator
{
    private DictionaryOfPoolsOfMonoBehaviour<Garbage> _pools;
    private List<(float time, Vector3 finalPosition, Vector3 initialPosition, Quaternion rotation, TrackPiece trackPiece, GameObject prefab)> _plannedSpawns = new();
    private Dictionary<GameObject, float> _prefabToGravity = new();

    public TrackObjectsInstantiator(Transform poolFolder, Transform outOfPoolFolder, TrackObstaclesGenerator obstaclesGenerator, TrackGarbageGenerator garbageGenerator)
    {
        _pools = new DictionaryOfPoolsOfMonoBehaviour<Garbage>(poolFolder, outOfPoolFolder);

        foreach (GarbageSpawningBeatStrength x in garbageGenerator.BeatStrengths)
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
    }

    public void CheckSpawnPlannedTrash()
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

    public void SpawnOrPlanToThrowObject(GameObject prefab, int beat, int lane, TrackPiece trackPiece, float oddsSpawnImmediately, float warningTime, float yOffset)
    {
        float distanceAlongMidline = beat * TrackPiece.TRACK_PIECE_LENGTH / 16;
        CalcPositionAndRotationForObjectOnTrack(trackPiece, distanceAlongMidline, lane, yOffset, out Vector3 finalPosition, out Quaternion rotation);

        if (Random.value <= oddsSpawnImmediately)
        {
            Spawn(prefab, finalPosition, rotation, trackPiece, false, Vector3.negativeInfinity);
        }
        else
        {
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

    private void CalcPositionAndRotationForObjectOnTrack(TrackPiece trackPiece, float distanceAlongMidline, float lane
        , float yOffset
        , out Vector3 position, out Quaternion rotation)
    {
        trackPiece.StoreLane(0);
        float t = trackPiece.FindTForDistanceAlongStoredLane(distanceAlongMidline, 0f);

        Vector3 midlinePosition = trackPiece.BezierCurve(t) + Vector3.up * yOffset;

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
