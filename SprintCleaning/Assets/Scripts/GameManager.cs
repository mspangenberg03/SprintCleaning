using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject[] _trackPrefabs;
    [SerializeField] private GameObject _player;

    [SerializeField]
    private TrackTurn[] _turns;

    [System.Serializable]
    private class TrackTurn
    {
        public Vector3 _positionOffset;
        public float _yRotationOffset;
    }

    private Transform[] TrackPieces => TrackPositions.Instance.TrackPoints;
    private static GameManager _instance;
    //private Vector3 _trackPieceOffset = new Vector3(0, 0, 10);

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

    public void InstantiateTrackSegment()
    {

        //Destroys the trackPieces as the player gets to the checkPoint in the middle of the next
        GameObject.Destroy(TrackPieces[0].GetComponentsInParent<Transform>()[1].gameObject);
        //Creates a trackPiece following the last created
        int indexToInstanciate = TrackRandomIndex();

        TrackTurn trackTurn = _turns[Random.Range(0, _turns.Length)];

        Vector3 newPosition = TrackPieces[TrackPieces.Length - 1].TransformPoint(trackTurn._positionOffset);

        float priorYRotation = TrackPieces[TrackPieces.Length - 1].rotation.eulerAngles.y;
        Quaternion newRotation = Quaternion.Euler(0, priorYRotation + trackTurn._yRotationOffset, 0);

        GameObject newTrackPiece = Instantiate(_trackPrefabs[indexToInstanciate], newPosition, newRotation);


        for (int i = 0; i < TrackPieces.Length - 1; i++)
        {
            TrackPieces[i] = TrackPieces[i + 1];
        }

        //Inserts the trackPiece just created in the array of trackPoints the player is following
        TrackPieces[TrackPieces.Length - 1] = newTrackPiece.GetComponentInChildren<Transform>().GetChild(0);
    }
 }
