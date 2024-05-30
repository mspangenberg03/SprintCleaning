using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject[] _trackPrefabs;
    [SerializeField] private GameObject _player;
    [SerializeField] private float _oddsDontTurn = .8f;

    [SerializeField]
    private TrackTurn[] _turns; // index 0 must be straight

    [System.Serializable]
    private class TrackTurn
    {
        public Vector3 _positionOffset;
        public float _yRotationOffset;
    }

    private int _priorTurnIndex; 

    private Transform[] TrackPieces => TrackPositions.Instance.TrackPoints;
    private static GameManager _instance;
    //private Vector3 _trackPieceOffset = new Vector3(0, 0, 10);

    public static GameManager Instance
    {
        get{
        if(_instance == null){
            _instance = FindObjectOfType<GameManager>();
            //GameObject gameManager = new GameObject("GameManager");
            //gameManager.AddComponent<GameManager>();
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

    public void InstantiateTrackSegment()
    {

        //Destroys the trackPieces as the player gets to the checkPoint in the middle of the next
        GameObject.Destroy(TrackPieces[0].GetComponentsInParent<Transform>()[1].gameObject);
        //Creates a trackPiece following the last created
        int indexToInstanciate = TrackRandomIndex();

        int count = 0;
        int turnIndex;
        do
        {
            count++;
            if (count == 100)
            {
                throw new System.Exception();
            }
            turnIndex = Random.Range(0, _turns.Length);
        } while (!(turnIndex == 0 || turnIndex != _priorTurnIndex));

        if (Random.value < _oddsDontTurn)
        {
            turnIndex = 0;
        }

        _priorTurnIndex = turnIndex;

        TrackTurn trackTurn = _turns[turnIndex];

        Vector3 newPosition = TrackPieces[TrackPieces.Length - 1].TransformPoint(trackTurn._positionOffset);

        float priorYRotation = TrackPieces[TrackPieces.Length - 1].rotation.eulerAngles.y;
        Quaternion newRotation = Quaternion.Euler(0, priorYRotation + trackTurn._yRotationOffset, 0);

        GameObject newTrackPiece = Instantiate(_trackPrefabs[indexToInstanciate], newPosition, newRotation);
        GetComponent<ItemSpawner>().SpawnItems(newTrackPiece);


        for (int i = 0; i < TrackPieces.Length - 1; i++)
        {
            TrackPieces[i] = TrackPieces[i + 1];
        }

        //Inserts the trackPiece just created in the array of trackPoints the player is following
        TrackPieces[TrackPieces.Length - 1] = newTrackPiece.GetComponentInChildren<Transform>().GetChild(0);
    }
 }
