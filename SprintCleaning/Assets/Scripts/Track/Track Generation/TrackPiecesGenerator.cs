using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TrackPiecesGenerator
{
    [Header("Index 0 of Track Prefabs must be the straight track piece.")]
    [SerializeField] private GameObject[] _trackPrefabs;
    [SerializeField] private float _oddsDontTurn = .8f;
    [SerializeField] private float _minStraightBetweenTurns = 2;

    private int _numTrackPieces;
    private ArrayOfPoolsOfMonoBehaviour<TrackPiece> _trackPiecePools;
    private int _priorTrackPieceIndex;
    private int _numStraightSinceLastTurn;
    private List<TrackPiece> _trackPieces;

    public void Initialize(Transform poolFolder, Transform outOfPoolFolder, List<TrackPiece> trackPieces, int numTrackPieces)
    {
        _trackPiecePools = new ArrayOfPoolsOfMonoBehaviour<TrackPiece>(_trackPrefabs, poolFolder, outOfPoolFolder);
        _trackPieces = trackPieces;
        _numTrackPieces = numTrackPieces;
    }

    public TrackPiece AddTrackPiece()
    {
        int prefabIndex = NextTrackPieceIndex();

        TrackPiece newTrackPiece = _trackPiecePools.Produce(prefabIndex, Vector3.down * PlayerMovement.Settings.PlayerVerticalOffset, Quaternion.identity);

        if (newTrackPiece.Prior != null)
        {
            // Choose a position and rotation such that the Start transform of the new track piece has the same position and rotation as the prior
            // track piece's End transform.
            Transform continueFrom = newTrackPiece.Prior.EndTransform;

            Vector3 rotationChange = continueFrom.rotation.eulerAngles - newTrackPiece.StartTransform.rotation.eulerAngles;
            newTrackPiece.transform.Rotate(rotationChange);

            Vector3 positionChange = continueFrom.position - newTrackPiece.StartTransform.position;
            newTrackPiece.transform.position += positionChange;
        }

        _trackPieces.Add(newTrackPiece);

        if (_trackPieces.Count > _numTrackPieces)
        {
            _trackPiecePools.ReturnToPool(_trackPieces[0]); // This also returns the objects on the track to the pool, via TrackPiece.OnReturnToPool().
            _trackPieces.RemoveAt(0);
        }

        return newTrackPiece;
    }

    private int NextTrackPieceIndex()
    {
        int index;

        if (_trackPrefabs.Length == 1 || _trackPieces.Count == 0
            || _numStraightSinceLastTurn < _minStraightBetweenTurns || Random.value < _oddsDontTurn)
        {
            // Track doesn't turn
            index = 0;
            if (_trackPieces.Count != 0)
                _numStraightSinceLastTurn++;
        }
        else
        {
            // Track turns
            _numStraightSinceLastTurn = 0;
            do
            {
                index = Random.Range(1, _trackPrefabs.Length);
            } while (index == _priorTrackPieceIndex);
        }

        _priorTrackPieceIndex = index;

        return index;
    }

}
