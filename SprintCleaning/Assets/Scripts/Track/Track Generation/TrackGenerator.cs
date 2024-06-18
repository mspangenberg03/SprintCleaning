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

    void Awake()
    {
        _instance = this;

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
        _rightBuildingsGenerator = new TrackBuildingsGeneratorOneSide(buildingPoolFolder, buildingFolder, false, _buildingsGeneratorSettings);
        _leftBuildingsGenerator = new TrackBuildingsGeneratorOneSide(buildingPoolFolder, buildingFolder, true, _buildingsGeneratorSettings);

        for (int i = 0; i < _numTrackPieces; i++)
            AddTrackPiece();
    }

    public void AddTrackPiece()
    {
        TrackPiece newTrackPiece = _trackPiecesGenerator.AddTrackPiece();
        if (newTrackPiece.Prior != null)
        {
            // Generate buildings 1 track piece later so can check for overlap with the next track piece.
            // Generating garbage 2 track pieces later so if the nearest throwing point to a garbage's point on the track is on a building from the next track piece,
            // that building will already exist.

            _rightBuildingsGenerator.AddBuildings(newTrackPiece.Prior);
            _leftBuildingsGenerator.AddBuildings(newTrackPiece.Prior);

            if (newTrackPiece.Prior.Prior != null)
                _trackObjectsGenerator.AddTrash(newTrackPiece.Prior.Prior, TrackPieces.Count);
        }
    }

    public void AfterPlayerMovementFixedUpdate()
    {
        _trackObjectsGenerator.AfterPlayerMovementFixedUpdate();
    }
}
