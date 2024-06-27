using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackGenerator : MonoBehaviour
{
    [SerializeField] private bool _spawnPowerups;
    [SerializeField] private int _numTrackPieces = 10;
    [SerializeField] private TrackPiecesGenerator _trackPiecesGenerator;
    [SerializeField] private TrackGarbageGenerator _garbageGenerator;
    [SerializeField] private TrackObstaclesGenerator _obstaclesGenerator;
    [SerializeField] private TrackPowerupsGenerator _powerupsGenerator;
    [SerializeField] private BuildingsGeneratorInspectorSettings _buildingsGeneratorSettings;

    private TrackBuildingsGeneratorOneSide _rightBuildingsGenerator;
    private TrackBuildingsGeneratorOneSide _leftBuildingsGenerator;
    private TrackObjectsInstantiator _trackObjectsInstantiator;


    public List<TrackPiece> TrackPieces { get; private set; } = new();

    private static TrackGenerator _instance;
    public static TrackGenerator Instance 
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<TrackGenerator>();
            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;

        // These folder gameObjects are just to make the hierarchy window more organized.
        Transform trackPieceFolder = new GameObject("Track Pieces").transform;
        Transform trackObjectFolder = new GameObject("Track Objects").transform;
        Transform buildingFolder = new GameObject("Buildings").transform;

        Transform trackPiecePoolFolder = new GameObject("Track Pieces Pool").transform;
        Transform trackObjectPoolFolder = new GameObject("Track Objects Pool").transform;
        Transform buildingPoolFolder = new GameObject("Buildings Pool").transform;

        trackPieceFolder.parent = transform;
        trackObjectFolder.parent = transform;
        buildingFolder.parent = transform;

        trackPiecePoolFolder.parent = transform;
        trackObjectPoolFolder.parent = transform;
        buildingPoolFolder.parent = transform;


        _trackPiecesGenerator.Initialize(trackPiecePoolFolder, trackPieceFolder, TrackPieces, _numTrackPieces);
        _trackObjectsInstantiator = new TrackObjectsInstantiator(trackObjectPoolFolder, trackObjectFolder, _obstaclesGenerator, _garbageGenerator, _powerupsGenerator);
        _obstaclesGenerator.Initialize(_trackObjectsInstantiator);
        _powerupsGenerator.Initialize(_trackObjectsInstantiator);
        _garbageGenerator.Initialize(_trackObjectsInstantiator);


        _rightBuildingsGenerator = new TrackBuildingsGeneratorOneSide(buildingPoolFolder, buildingFolder, _buildingsGeneratorSettings, false);
        _leftBuildingsGenerator = new TrackBuildingsGeneratorOneSide(buildingPoolFolder, buildingFolder, _buildingsGeneratorSettings, true);

        for (int i = 0; i < _numTrackPieces; i++)
            AddTrackPieceAndObjects();
    }

    public void AddTrackPieceAndObjects()
    {
        TrackPiece newTrackPiece = _trackPiecesGenerator.AddTrackPiece();

        _rightBuildingsGenerator.AddBuildings(newTrackPiece);
        _leftBuildingsGenerator.AddBuildings(newTrackPiece);

        // Add trash on the 2nd to last track piece, because buildings need to finish spawning up to some distance
        // away, in order to decide where trash will be thrown from.
        TrackPiece addTrashOn = newTrackPiece.Prior;
        if (addTrashOn != null)
        {
            bool spawnNone = TrackPieces.Count < 4; // To prevent immediately encountering trash when the run starts.
            _garbageGenerator.AddGarbage(addTrashOn, spawnNone, out var selectedBeatsAndPrefabsAndLanesForGarbage);
            _obstaclesGenerator.AddObstaclesToTrackPiece(addTrashOn, spawnNone, selectedBeatsAndPrefabsAndLanesForGarbage);
            if (_spawnPowerups)
                _powerupsGenerator.AddPowerupsToTrackPiece(addTrashOn, spawnNone, selectedBeatsAndPrefabsAndLanesForGarbage);
        }
    }

    public void AfterPlayerMovementFixedUpdate()
    {
        _trackObjectsInstantiator.CheckSpawnPlannedTrash();

        for (int i = Garbage.ThrownGarbage.Count - 1; i >= 0; i--)
            Garbage.ThrownGarbage[i].MoveWhileBeingThrown();
    }

}
