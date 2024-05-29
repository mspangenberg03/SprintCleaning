using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TrackGenerator : MonoBehaviour
{
    [SerializeField] private GameObject[] _trackPrefabs; // index 0 should be the straight track piece
    [SerializeField] private GameObject _player;
    [SerializeField] private int _numTrackPoints = 10;
    [SerializeField] private float _oddsDontTurn = .8f;
    [SerializeField] private float _minStraightBetweenTurns = 2;

    private int _priorTrackPieceIndex;
    private int _numStraightSinceLastTurn;



    private static TrackGenerator _instance;
    public static TrackGenerator Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<TrackGenerator>();
            }
            return _instance;
        }
    }

    public List<TrackPiece> TrackPieces { get; private set; } = new();


    void Awake()
    {
        _instance = this;
        for (int i = 0; i < _numTrackPoints; i++)
        {
            AddTrackPiece();
        }
    }

    public void AddTrackPiece()
    {
        if (TrackPieces.Count == 0)
        {
            CreateFirstTrackPiece();
            return;
        }



       

        //Creates a trackPiece following the last created
        GameObject prefab = RandomTrackPiecePrefab();

        // Choose a position and rotation such that the Start transform of the new track piece has the same position and rotation as the prior
        // track piece's End transform.

        GameObject instantiated = Instantiate(prefab, transform);
        TrackPiece newTrackPiece = instantiated.GetComponent<TrackPiece>();
        TrackPiece priorTrackPiece = TrackPieces[TrackPieces.Count - 1];   

        Vector3 rotationChange = priorTrackPiece.EndTransform.rotation.eulerAngles - newTrackPiece.StartTransform.rotation.eulerAngles;
        instantiated.transform.Rotate(rotationChange);

        Vector3 positionChange = priorTrackPiece.EndTransform.position - newTrackPiece.StartTransform.position;
        instantiated.transform.position += positionChange;


        //Inserts the trackPiece just created in the array of trackPoints the player is following
        TrackPieces.Add(newTrackPiece);

        if (TrackPieces.Count > _numTrackPoints)
        {
            //Destroys the trackPieces as the player gets to the checkPoint in the middle of the next
            Destroy(TrackPieces[0].gameObject);
            TrackPieces.RemoveAt(0);
        }

    }

    private void CreateFirstTrackPiece()
    {
        GameObject newTrackPiece = Instantiate(RandomTrackPiecePrefab()
            , Vector3.down * TrackPositions.Instance.PlayerVerticalOffset, Quaternion.identity);

        TrackPieces.Add(newTrackPiece.GetComponent<TrackPiece>());
    }

    private GameObject RandomTrackPiecePrefab()
    {
        int index;

        if (_numStraightSinceLastTurn < _minStraightBetweenTurns || Random.value < _oddsDontTurn)
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

        _priorTrackPieceIndex = index;

        return _trackPrefabs[index];
    }

}
