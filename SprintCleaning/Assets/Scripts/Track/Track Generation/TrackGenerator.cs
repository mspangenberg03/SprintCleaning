using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackGenerator : MonoBehaviour
{
    [SerializeField] private int _numTrackPieces = 10;
    [SerializeField] private TrackPiecesGenerator _trackPiecesGenerator;
    [SerializeField] private TrackObjectsGenerator _trackObjectsGenerator;
    [SerializeField] private BuildingsGeneratorInspectorSettings _buildingsGeneratorSettings;

    private TrackBuildingsGeneratorOneSide _rightBuildingsGenerator;
    private TrackBuildingsGeneratorOneSide _leftBuildingsGenerator;

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
        _trackObjectsGenerator.Initialize(trackObjectPoolFolder, trackObjectFolder);
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
            _trackObjectsGenerator.AddTrash(addTrashOn, TrackPieces.Count);
    }

    public void AfterPlayerMovementFixedUpdate()
    {
        _trackObjectsGenerator.AfterPlayerMovementFixedUpdate();
    }
}
