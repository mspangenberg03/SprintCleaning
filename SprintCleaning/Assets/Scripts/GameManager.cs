using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject[] _trackPrefabs;
    [SerializeField] private GameObject _player;
    [SerializeField] private int _numTrackPoints = 10;
    [SerializeField] private float _oddsDontTurn = .8f;
    [SerializeField] private float _minStraightBetweenTurns = 2;

    [SerializeField]
    private TrackTurn[] _turns; // index 0 must be straight

    [System.Serializable]
    private class TrackTurn
    {
        public Vector3 _positionOffset;
        public float _yRotationOffset;
    }

    private int _priorTurnIndex;
    private int _numStraightSinceLastTurn;


    private List<Transform> TrackPieces => TrackPositions.Instance.TrackPoints;
    private static GameManager _instance;
    //private Vector3 _trackPieceOffset = new Vector3(0, 0, 10);

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
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
        for (int i = 0; i < _numTrackPoints; i++)
        {
            AddTrackPiece();
        }
    }

    private int TrackRandomIndex()
    {
        return Random.Range(0, _trackPrefabs.Length);
    }

    public void AddTrackPiece()
    {
        if (TrackPieces.Count == 0)
        {
            CreateFirstTrackPiece();
            return;
        }



       

        //Creates a trackPiece following the last created
        int indexToInstanciate = TrackRandomIndex();

        TrackTurn trackTurn = RandomTurn();

        Vector3 newPosition = TrackPieces[TrackPieces.Count - 1].TransformPoint(trackTurn._positionOffset);

        float priorYRotation = TrackPieces[TrackPieces.Count - 1].rotation.eulerAngles.y;
        Quaternion newRotation = Quaternion.Euler(0, priorYRotation + trackTurn._yRotationOffset, 0);

        GameObject newTrackPiece = Instantiate(_trackPrefabs[indexToInstanciate], newPosition, newRotation, transform);


        //Inserts the trackPiece just created in the array of trackPoints the player is following
        TrackPieces.Add(newTrackPiece.transform.GetChild(0));

        if (TrackPieces.Count > _numTrackPoints)
        {
            //Destroys the trackPieces as the player gets to the checkPoint in the middle of the next
            Destroy(TrackPieces[0].parent.gameObject);
            TrackPieces.RemoveAt(0);
        }

    }

    private void CreateFirstTrackPiece()
    {
        GameObject newTrackPiece = Instantiate(RandomTrackPiecePrefab()
            , Vector3.down * TrackPositions.Instance.PlayerVerticalOffset, Quaternion.identity);

        TrackPieces.Add(newTrackPiece.transform.GetChild(0));
    }

    private TrackTurn RandomTurn()
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
                index = Random.Range(1, _turns.Length);
            } while (index == _priorTurnIndex);
        }

        _priorTurnIndex = index;

        return _turns[index];
    }



    private GameObject RandomTrackPiecePrefab() => _trackPrefabs[Random.Range(0, _trackPrefabs.Length)];

}
