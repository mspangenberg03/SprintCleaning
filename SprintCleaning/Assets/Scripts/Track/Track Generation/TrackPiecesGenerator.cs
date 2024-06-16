using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TrackPiecesGenerator
{
    [Header("Track")]
    [SerializeField] private GameObject[] _trackPrefabs; // index 0 should be the straight track piece
    [SerializeField] private float _oddsDontTurn = .8f;
    [SerializeField] private float _minStraightBetweenTurns = 2;

    private int _numTrackPieces;
    private ArrayOfPoolsOfMonoBehaviourOnGameObject<TrackPiece> _trackPiecePools;
    private int _priorTrackPieceIndex;
    private int _numStraightSinceLastTurn;
    private List<TrackPiece> _trackPieces;

    public void Initialize(Transform instantiatedGameObjectsParent, List<TrackPiece> trackPieces, int numTrackPieces)
    {
        _trackPiecePools = new ArrayOfPoolsOfMonoBehaviourOnGameObject<TrackPiece>(_trackPrefabs, instantiatedGameObjectsParent);
        _trackPieces = trackPieces;
        _numTrackPieces = numTrackPieces;
    }

    public TrackPiece AddTrackPiece()
    {
        int prefabIndex = NextTrackPieceIndex();

        TrackPiece newTrackPiece = _trackPiecePools.Produce(prefabIndex, Vector3.down * PlayerMovement.Settings.PlayerVerticalOffset, Quaternion.identity, out _);

        if (_trackPieces.Count > 0)
        {
            // Choose a position and rotation such that the Start transform of the new track piece has the same position and rotation as the prior
            // track piece's End transform.
            Transform continueFrom = _trackPieces[^1].EndTransform;

            Vector3 rotationChange = continueFrom.rotation.eulerAngles - newTrackPiece.StartTransform.rotation.eulerAngles;
            newTrackPiece.transform.Rotate(rotationChange);

            Vector3 positionChange = continueFrom.position - newTrackPiece.StartTransform.position;
            newTrackPiece.transform.position += positionChange;
        }

        _trackPieces.Add(newTrackPiece);

        if (_trackPieces.Count > _numTrackPieces)
        {
            _trackPiecePools.ReturnToPool(_trackPieces[0]);
            _trackPieces.RemoveAt(0);

            // need to do this elsewhere now
            //foreach (GameObject g in _spawnedObjects[0])
            //{
            //    if (g != null) // could've been destroyed by the player already
            //        Object.Destroy(g);
            //}
            //_spawnedObjects[0].Clear();
            //_poolOfListsOfGameObjects.ReturnToPool(_spawnedObjects[0]);
            //_spawnedObjects.RemoveAt(0);
        }

        return newTrackPiece;
    }

    private int NextTrackPieceIndex()
    {
        int index;

        if (_trackPrefabs.Length == 1)
            index = 0;
        else
        {
            if (_numStraightSinceLastTurn < _minStraightBetweenTurns || Random.value < _oddsDontTurn
                || _trackPieces.Count == 0)
            {
                // Track doesn't turn
                index = 0;
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
        }

        _priorTrackPieceIndex = index;

        return index;
    }

}
