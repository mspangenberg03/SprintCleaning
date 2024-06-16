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
        _trackPiecesGenerator.Initialize(transform, TrackPieces, _numTrackPieces);
        _trackObjectsGenerator.Initialize(transform);

        for (int i = 0; i < _numTrackPieces; i++)
            AddTrackPiece();
    }

    public void AddTrackPiece()
    {
        TrackPiece newTrackPiece = _trackPiecesGenerator.AddTrackPiece();
        _trackObjectsGenerator.AddTrash(newTrackPiece, TrackPieces.Count);
    }
}
