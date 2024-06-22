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
    private TrackPiece _currentPiece;
    private float _lengthOfCurrentPieceFilled;
    private bool _isLeft;

    private int _nextBuildingPrefabIndex = -1;
    private float[] _buildingWidths;

    private float _spawnpointLane;
    private float _noCrossingLane;


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

    public void AddBuildings(TrackPiece priorOfNewTrackPiece)
    {
        priorOfNewTrackPiece.StoreLane(_spawnpointLane);


        if (_currentPiece == null)
            _currentPiece = priorOfNewTrackPiece;

        int infiniteLoopCheck = 1000;
        while (infiniteLoopCheck > 0 && TryAddBuilding())
            infiniteLoopCheck--;

#if UNITY_EDITOR
        if (infiniteLoopCheck == 0)
            Debug.LogError("infinite loop detected");
        if (_currentPiece == null)
            throw new System.Exception("_currentPiece is null");
#endif
    }

    private bool TryAddBuilding()
    {
        _currentPiece.StoreLane(_spawnpointLane);

        if (_nextBuildingPrefabIndex == -1)
            _nextBuildingPrefabIndex = Random.Range(0, _buildingPrefabs.Length);

        // Stop trying to add buildings if there's no room for the next one.
        float nextBuildingWidth = _buildingWidths[_nextBuildingPrefabIndex];
        if (_currentPiece.Next == null && nextBuildingWidth > _currentPiece.ApproximateLengthForStoredLane() - _lengthOfCurrentPieceFilled)
            return false;

        // Find which track piece the building would spawn next to.
        TrackPiece spawnNextTo = _currentPiece;
        float lengthFromStartOfSpawnNextToToBuildingHalfWidth = _lengthOfCurrentPieceFilled + nextBuildingWidth / 2;
        while (lengthFromStartOfSpawnNextToToBuildingHalfWidth > spawnNextTo.ApproximateLengthForStoredLane())
        {
            lengthFromStartOfSpawnNextToToBuildingHalfWidth -= spawnNextTo.ApproximateLengthForStoredLane();
            spawnNextTo = spawnNextTo.Next;

            if (spawnNextTo == null)
                return false;

            spawnNextTo.StoreLane(_spawnpointLane);
        }
        float tAtSpawnPoint = spawnNextTo.FindTForDistanceAlongStoredLane(lengthFromStartOfSpawnNextToToBuildingHalfWidth, 0);

        Vector2 buildingDirection = spawnNextTo.BezierCurveDerivative(tAtSpawnPoint).To2D().normalized;
        Vector3 buildingSpawnPoint = spawnNextTo.BezierCurve(tAtSpawnPoint);
        Vector2 buildingSpawnPoint2D = buildingSpawnPoint.To2D();
        Vector2 earlierCorner = buildingSpawnPoint2D - nextBuildingWidth / 2 * buildingDirection;
        Vector2 laterCorner = buildingSpawnPoint2D + nextBuildingWidth / 2 * buildingDirection;
        if (_isLeft)
        {
            Vector2 temp = earlierCorner;
            earlierCorner = laterCorner;
            laterCorner = temp;
        }

        spawnNextTo.StoreLane(_noCrossingLane);
        if (spawnNextTo.Prior != null)
            spawnNextTo.Prior.StoreLane(_noCrossingLane);
        if (spawnNextTo.Next != null)
            spawnNextTo.Next.StoreLane(_noCrossingLane);

        if (spawnNextTo.IntersectsWithLine2D(earlierCorner, laterCorner)
            || (spawnNextTo.Prior != null && spawnNextTo.Prior.IntersectsWithLine2D(earlierCorner, laterCorner))
            || (spawnNextTo.Next != null && spawnNextTo.Next.IntersectsWithLine2D(earlierCorner, laterCorner)))
        {
            // The building would intersect with the track, so move the starting point forwards and try again
            _lengthOfCurrentPieceFilled += 1f;
            return true;
        }

        // Spawn the building
        float yRotation = -Vector2.SignedAngle(Vector2.up, buildingDirection) - 90 + (_isLeft ? 180 : 0);
        Quaternion rotation = Quaternion.Euler(0, yRotation, 0);
        Building newBuilding = _buildingPools.Produce(_nextBuildingPrefabIndex, buildingSpawnPoint + Vector3.up * PlayerMovement.Settings.PlayerVerticalOffset, rotation);




        spawnNextTo.BuildingsByThisTrackPiece.Add(newBuilding);

        _currentPiece = spawnNextTo;

        _lengthOfCurrentPieceFilled = lengthFromStartOfSpawnNextToToBuildingHalfWidth + _buildingWidths[_nextBuildingPrefabIndex] / 2;
        while (true)
        {
            _currentPiece.StoreLane(_spawnpointLane);

            float lengthOfCurrentPiece = _currentPiece.ApproximateLengthForStoredLane();
            if (_lengthOfCurrentPieceFilled < lengthOfCurrentPiece)
                break;
            _lengthOfCurrentPieceFilled -= lengthOfCurrentPiece;
            _currentPiece = _currentPiece.Next;
        }

        _nextBuildingPrefabIndex = -1;

        return true;

    }
}
