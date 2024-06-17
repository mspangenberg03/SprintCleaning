using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackGenerator : MonoBehaviour
{
    [SerializeField] private int _numTrackPieces = 10;
    [SerializeField] private TrackPiecesGenerator _trackPiecesGenerator;
    [SerializeField] private TrackObjectsGenerator _trackObjectsGenerator;
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

        for (int i = 0; i < _numTrackPieces; i++)
            AddTrackPiece();
    }

    public void AddTrackPiece()
    {
        TrackPiece newTrackPiece = _trackPiecesGenerator.AddTrackPiece();
        _trackObjectsGenerator.AddTrash(newTrackPiece, TrackPieces.Count);
    }
}
