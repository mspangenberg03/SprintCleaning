using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject[] _trackPrefabs;
    [SerializeField] private GameObject _player;
    private PlayerMovement _playerMov => _player.GetComponent<PlayerMovement>();
    private Transform[] _trackPieces => _playerMov._trackPoints;
    //private int _lastTrackPieceInstanciatedIndex = 3;
    private static GameManager _instance;
    private Vector3 _trackPieceOffset = new Vector3(0, 0, 10);

    public static GameManager Instance
    {
        get{
        if(_instance is null){
            GameObject gameManager = new GameObject("GameManager");
            gameManager.AddComponent<GameManager>();
        }
        return _instance;
        }
    }
    void Awake()
    {
        _instance = this;
    }

    private int TrackRandomIndex()
    {
        return Random.Range(0, _trackPrefabs.Length);
    }

    public void InstantiateTrackSegment(int lastTrackPointIndex)
    {
        //Destroys the trackPieces as the player gets to the checkPoint in the middle of the next
        GameObject.Destroy(_trackPieces[0].GetComponentsInParent<Transform>()[1].gameObject);
        //Creates a trackPiece following the last created
        int indexToInstanciate = TrackRandomIndex();
        GameObject newTrackPiece = Instantiate(_trackPrefabs[indexToInstanciate], _trackPieces[_trackPieces.Length - 1].position + 
                                                _trackPieceOffset,_trackPrefabs[indexToInstanciate].transform.rotation);


        for (int i = 0; i < _trackPieces.Length - 1; i++)
        {
            _trackPieces[i] = _trackPieces[i + 1];
        }

        //Inserts the trackPiece just created in the array of trackPoints the player is following
        _trackPieces[_trackPieces.Length - 1] = newTrackPiece.GetComponentInChildren<Transform>().GetChild(0);
        //_lastTrackPieceInstanciatedIndex ++;
        //if(_lastTrackPieceInstanciatedIndex == _trackPieces.Length){
        //    _lastTrackPieceInstanciatedIndex = 0;
        //}

        // a b c d
        // b c d d

        //// Move track piece at index 0 to the last index
        //Transform newPiece = _trackPieces[0];
        //for (int i = 0; i < _trackPieces.Length - 1; i++)
        //{
        //    _trackPieces[i] = _trackPieces[i + 1];
        //}
        //_trackPieces[_trackPieces.Length - 1] = newPiece; 
    }
 }
