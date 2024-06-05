using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TrackGenerator : MonoBehaviour
{
    private const float STANDARD_TRACK_PIECE_LENGTH = 10f;

    [Header("Track")]
    [SerializeField] private GameObject[] _trackPrefabs; // index 0 should be the straight track piece
    [SerializeField] private int _numTrackPoints = 10;
    [SerializeField] private float _oddsDontTurn = .8f;
    [SerializeField] private float _minStraightBetweenTurns = 2;
    [Header("Objects on Track")]
    [SerializeField] private float _minObjectSeparation; // objects meaning trash and tools
    //[SerializeField] private float _minDistanceFromTrackLayerCollider; 
    // ^ will need this to prevent spawning inside the track at the edge of flat and upwards slope.
    // To do that, will need to use physics layers and add colliders to the track pieces. Might as well wait until we have
    // models for track pieces so we don't need to refit the colliders (?).
    [SerializeField] private float _trackObjectsYOffset = 1.5f;
    [SerializeField] private FloatRange _trashCountPerStandardLength;
    [SerializeField] private FloatRange _toolCountPerStandardLength;
    [System.Serializable] private class FloatRange { public float min; public float max; }
    [SerializeField] private GameObject[] _trashPrefabs;

    private int _totalTrackPieces;
    private int _priorTrackPieceIndex;
    private int _numStraightSinceLastTurn;
    private float _trashLeftover;
    private List<List<GameObject>> _spawnedObjects = new();
    private List<List<GameObject>> _gameObjectListPool = new();
    public List<TrackPiece> TrackPieces { get; private set; } = new();

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
        _totalTrackPieces++;

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

        TrackPieces.Add(newTrackPiece);
        AddTrashAndTools(newTrackPiece);

        if (TrackPieces.Count > _numTrackPoints)
        {
            // destroy the earliest track piece and all objects spawned on it
            Destroy(TrackPieces[0].gameObject);
            TrackPieces.RemoveAt(0);

            List<GameObject> objectsToDestroy = _spawnedObjects[0];
            foreach (GameObject g in objectsToDestroy)
            {
                if (g != null) // could've been destroyed by the player already
                    Destroy(g);
            }
            objectsToDestroy.Clear();
            _gameObjectListPool.Add(objectsToDestroy);
            _spawnedObjects.RemoveAt(0);
        }

    }

    private void AddTrashAndTools(TrackPiece trackPiece)
    {
        // Create or reuse a list to store the objects on this trackPiece
        List<GameObject> gameObjectsOnNewTrackPiece;
        if (_gameObjectListPool.Count == 0)
            gameObjectsOnNewTrackPiece = new List<GameObject>();
        else
        {
            gameObjectsOnNewTrackPiece = _gameObjectListPool[_gameObjectListPool.Count - 1];
            _gameObjectListPool.RemoveAt(_gameObjectListPool.Count - 1);
        }
        _spawnedObjects.Add(gameObjectsOnNewTrackPiece);

        // Determine how many trashes and how many tools.
        trackPiece.StoreLane(0);
        float numStandardLengths = trackPiece.ApproximateCurveLength() / STANDARD_TRACK_PIECE_LENGTH;
        float numTrashFloat = Random.Range(_trashCountPerStandardLength.min, _trashCountPerStandardLength.max) * numStandardLengths + _trashLeftover;
        int numTrash = (int)numTrashFloat;
        _trashLeftover = numTrashFloat - numTrash;

        if (_totalTrackPieces < 5)
        {
            numTrash = 0;
        }

        // Add trash pieces
        for (int i = 0; i < numTrash; i++)
        {
            GameObject prefab = _trashPrefabs[Random.Range(0, _trashPrefabs.Length)]; // Could do a random bag to prevent too many of the same type of trash
            Vector3 position = ChooseRandomPositionAndRotationForObjectOnTrack(trackPiece, out Quaternion rotation);
            if (float.IsNaN(position.x))
                break; // Couldn't find a valid position
            //Quaternion rotation = Quaternion.Euler(0, Random.Range(-180f, 180f), 0f);
            GameObject instantiated = Instantiate(prefab, position, rotation, transform);
            gameObjectsOnNewTrackPiece.Add(instantiated);
        }
    }

    private Vector3 ChooseRandomPositionAndRotationForObjectOnTrack(TrackPiece trackPiece, out Quaternion rotation)
    {
        int attemptsLeft = 100;
        while (true)
        {
            attemptsLeft--;
            if (attemptsLeft == 0)
                break;

            float lane = Random.Range(-1, 2);

            float t = (float)Random.Range(0, TrackPiece.TRACK_PIECE_LENGTH) / TrackPiece.TRACK_PIECE_LENGTH;

            trackPiece.StoreLane(lane);
            Vector3 position = trackPiece.BezierCurve(t) + Vector3.up * _trackObjectsYOffset;
            Vector3 direction = trackPiece.BezierCurveDerivative(t);
            Vector3 directionOnPlane = new Vector3(direction.x, 0, direction.z);
            float directionAngle = Quaternion.FromToRotation(Vector3.forward, directionOnPlane).eulerAngles.y;
            rotation = Quaternion.Euler(0f, directionAngle, 0f);

            bool invalid = false;
            for (int i = 0; i < _spawnedObjects.Count; i++)
            {
                foreach (GameObject g in _spawnedObjects[i])
                {
                    if (g == null)
                        continue; // it was destroyed
                    if ((g.transform.position - position).sqrMagnitude < _minObjectSeparation * _minObjectSeparation)
                    {
                        invalid = true;
                        break;
                    }
                }
                if (invalid)
                    break;
            }

            if (!invalid)
                return position;
        }
        rotation = Quaternion.identity;
        return new Vector3(float.NaN, float.NaN, float.NaN);
    }

    private void CreateFirstTrackPiece()
    {
        GameObject newTrackPiece = Instantiate(RandomTrackPiecePrefab()
            , Vector3.down * PlayerMovement.Settings.PlayerVerticalOffset, Quaternion.identity);

        TrackPieces.Add(newTrackPiece.GetComponent<TrackPiece>());
    }

    private GameObject RandomTrackPiecePrefab()
    {
        int index;

        if (_trackPrefabs.Length == 1)
            index = 0;
        else
        {
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
        }

        _priorTrackPieceIndex = index;

        return _trackPrefabs[index];
    }

}
