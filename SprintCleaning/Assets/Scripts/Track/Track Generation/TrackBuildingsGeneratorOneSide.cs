using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BuildingsGeneratorInspectorSettings
{
    [field: SerializeField] public float DistanceFromTrackAtMiddleOfWidth { get; private set; } = .1f;
    [field: SerializeField] public float MinDistanceFromTrackAlongWholeWidth { get; private set; } = 0;
    [field: SerializeField] public GameObject[] BuildingPrefabs { get; private set; }
}

public class TrackBuildingsGeneratorOneSide
{
    private GameObject[] _buildingPrefabs;
    private ArrayOfPoolsOfMonoBehaviour<Building> _buildingPools;
    private TrackPiece _trackPieceBeingFilled;
    private float _filledUpToLengthOfTrackPiece;
    private bool _isLeft;

    private int _nextBuildingPrefabIndex = -1;
    private float[] _buildingWidths;

    private float _spawnpointLane;
    private float _noCrossingLane;

    private Building _priorSpawned;


    public TrackBuildingsGeneratorOneSide(Transform poolFolder, Transform outOfPoolFolder, bool isLeft, BuildingsGeneratorInspectorSettings inspectorSettings)
    {
        _buildingPrefabs = inspectorSettings.BuildingPrefabs;
        _isLeft = isLeft;
        _buildingPools = new ArrayOfPoolsOfMonoBehaviour<Building>(_buildingPrefabs, poolFolder, outOfPoolFolder);

        _buildingWidths = new float[_buildingPrefabs.Length];
        for (int i = 0; i < _buildingPrefabs.Length; i++)
            _buildingWidths[i] = _buildingPrefabs[i].GetComponentInChildren<Building>().Width;

        _spawnpointLane = 1 + inspectorSettings.DistanceFromTrackAtMiddleOfWidth / PlayerMovement.Settings.DistanceBetweenLanes;
        _noCrossingLane = 1 + inspectorSettings.MinDistanceFromTrackAlongWholeWidth / PlayerMovement.Settings.DistanceBetweenLanes;
        if (_isLeft) 
        {
            _spawnpointLane = -_spawnpointLane;
            _noCrossingLane = -_noCrossingLane;
        }
    }

    public void AddBuildings(TrackPiece newTrackPiece)
    {
        newTrackPiece.StoreLane(_spawnpointLane);

        if (_trackPieceBeingFilled == null)
            _trackPieceBeingFilled = newTrackPiece;

        while (KeepTryingToAddBuilding()) ;
    }

    private bool KeepTryingToAddBuilding()
    {
        // If it hasn't picked a building prefab to spawn, pick one.
        if (_nextBuildingPrefabIndex == -1)
            _nextBuildingPrefabIndex = Random.Range(0, _buildingPrefabs.Length);

        // Find where to spawn the building along the track, starting from a minimum position.
        // If the track is too full, then it's done spawning buildings until another track piece gets added.
        float nextBuildingWidth = _buildingWidths[_nextBuildingPrefabIndex];
        if (!FindWhereToSpawnNextBuilding(out TrackPiece spawnTrackPiece, out float spawnDistanceAlongTrackPiece, nextBuildingWidth))
            return false; 

        float tAtSpawnPoint = spawnTrackPiece.FindTForDistanceAlongStoredLane(spawnDistanceAlongTrackPiece, 0);
        Vector2 buildingDirection = spawnTrackPiece.BezierCurveDerivative(tAtSpawnPoint).To2D().normalized;
        Vector3 buildingSpawnPoint = spawnTrackPiece.BezierCurve(tAtSpawnPoint);
        Vector2 buildingSpawnPoint2D = buildingSpawnPoint.To2D();
        
        // If the track curves a lot, the building might overlap the track, or at least get too close.
        // Check whether that's an issue and if so, move a little bit forwards along the track and then try again.
        if (WouldGetTooCloseToTrack(spawnTrackPiece, nextBuildingWidth, buildingDirection, buildingSpawnPoint2D))
        {
            // Move the starting point forwards and try again
            _filledUpToLengthOfTrackPiece += Random.value;
            return true;
        }

        // Buildings can overlap adjacent buildings if the track curves too much.
        // If that's an issue, move a little bit forwards along the track and then try again.
        SpawnBuildingWithoutOverlapping(buildingDirection, buildingSpawnPoint, out Building newBuilding, out bool failedOverlapCheck);
        if (failedOverlapCheck)
        {
            _filledUpToLengthOfTrackPiece += Random.value;
            return true;
        }
        spawnTrackPiece.BuildingsByThisTrackPiece.Add(newBuilding);

        // Successfully spawned the building, so move forwards along the track and then try to spawn another building.
        AdvanceAlongTrackWhenSpawnedNewBuilding(spawnTrackPiece, spawnDistanceAlongTrackPiece);
        _priorSpawned = newBuilding;
        _nextBuildingPrefabIndex = -1; // will pick one next time

        return true;
    }

    private bool FindWhereToSpawnNextBuilding(out TrackPiece trackPiece, out float distanceAlongTrackPiece, float nextBuildingWidth)
    {
        trackPiece = _trackPieceBeingFilled;
        distanceAlongTrackPiece = _filledUpToLengthOfTrackPiece + nextBuildingWidth / 2;

        trackPiece.StoreLane(_spawnpointLane);

        // Stop trying to add buildings if there's no room for the next one.
        if (trackPiece.Next == null && nextBuildingWidth > trackPiece.ApproximateLengthForStoredLane() - _filledUpToLengthOfTrackPiece)
            return false;

        // Find which track piece the building's middle would spawn next to.
        while (distanceAlongTrackPiece > trackPiece.ApproximateLengthForStoredLane())
        {
            distanceAlongTrackPiece -= trackPiece.ApproximateLengthForStoredLane();
            trackPiece = trackPiece.Next;

            if (trackPiece == null)
                return false;

            trackPiece.StoreLane(_spawnpointLane);
        }

        return true;
    }

    private bool WouldGetTooCloseToTrack(TrackPiece spawnTrackPiece, float nextBuildingWidth
       , Vector2 buildingDirection, Vector3 spawnPoint)
    {
        spawnTrackPiece.StoreLane(_noCrossingLane);
        if (spawnTrackPiece.Prior != null)
            spawnTrackPiece.Prior.StoreLane(_noCrossingLane);
        if (spawnTrackPiece.Next != null)
            spawnTrackPiece.Next.StoreLane(_noCrossingLane);

        Vector2 spawnPoint2D = spawnPoint.To2D();
        Vector2 earlierCorner = spawnPoint2D - nextBuildingWidth / 2 * buildingDirection * (_isLeft ? -1 : 1);
        Vector2 laterCorner = spawnPoint2D + nextBuildingWidth / 2 * buildingDirection * (_isLeft ? -1 : 1);

        return spawnTrackPiece.IntersectsWithLine2D(earlierCorner, laterCorner)
            || (spawnTrackPiece.Prior != null && spawnTrackPiece.Prior.IntersectsWithLine2D(earlierCorner, laterCorner))
            || (spawnTrackPiece.Next != null && spawnTrackPiece.Next.IntersectsWithLine2D(earlierCorner, laterCorner));
    }

    private void SpawnBuildingWithoutOverlapping(Vector2 buildingDirection, Vector3 buildingSpawnPoint, out Building newBuilding, out bool failedOverlapCheck)
    {
        float yRotation = -Vector2.SignedAngle(Vector2.up, buildingDirection) - 90 + (_isLeft ? 180 : 0);
        Quaternion rotation = Quaternion.Euler(0, yRotation, 0);
        newBuilding = _buildingPools.Produce(_nextBuildingPrefabIndex, buildingSpawnPoint + Vector3.up * PlayerMovement.Settings.PlayerVerticalOffset, rotation);

        failedOverlapCheck = _priorSpawned != null && Building.OverlappingFootprints(_priorSpawned, newBuilding);
        if (failedOverlapCheck)
            newBuilding.ReturnToPool();
    }

    private void AdvanceAlongTrackWhenSpawnedNewBuilding(TrackPiece spawnTrackPiece, float spawnDistanceAlongTrackPiece)
    {
        _trackPieceBeingFilled = spawnTrackPiece;

        _filledUpToLengthOfTrackPiece = spawnDistanceAlongTrackPiece + _buildingWidths[_nextBuildingPrefabIndex] / 2;
        while (true)
        {
            _trackPieceBeingFilled.StoreLane(_spawnpointLane);

            float lengthOfCurrentPiece = _trackPieceBeingFilled.ApproximateLengthForStoredLane();
            if (_filledUpToLengthOfTrackPiece < lengthOfCurrentPiece)
                break;
            _filledUpToLengthOfTrackPiece -= lengthOfCurrentPiece;
            _trackPieceBeingFilled = _trackPieceBeingFilled.Next;
        }
    }
}
