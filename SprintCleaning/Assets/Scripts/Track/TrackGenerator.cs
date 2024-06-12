using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackGenerator : MonoBehaviour
{
    [Header("Track")]
    [SerializeField] private GameObject[] _trackPrefabs; // index 0 should be the straight track piece
    [SerializeField] private int _numTrackPieces = 10;
    [SerializeField] private float _oddsDontTurn = .8f;
    [SerializeField] private float _minStraightBetweenTurns = 2;
    [Header("Objects on Track")]
    [SerializeField] private float _trackObjectsYOffset = 1.5f;
    [SerializeField] private int _minGarbageOnTrackPiece = 4;
    [SerializeField] private int _maxGarbageOnTrackPiece = 16;
    [Header("Beat strengths filled from 1st to last.")]
    [SerializeField] private GarbageSpawningBeatStrength[] _beatStrengths;

    private GameObject _trashPrefabForCheckingConsistentIntervals;
    private int _priorTrackPieceIndex;
    private int _numStraightSinceLastTurn;
    private List<List<GameObject>> _spawnedObjects = new();
    private ObjectPool<List<GameObject>> _listPool = new();
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
        _trashPrefabForCheckingConsistentIntervals = _beatStrengths[0].GarbagePrefabs[0];
        _instance = this;
        for (int i = 0; i < _numTrackPieces; i++)
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

        TrackPieces.Add(newTrackPiece);
        AddTrash(newTrackPiece);

        if (TrackPieces.Count > _numTrackPieces)
        {
            // destroy the earliest track piece and all objects spawned on it
            Destroy(TrackPieces[0].gameObject);
            TrackPieces.RemoveAt(0);

            foreach (GameObject g in _spawnedObjects[0])
            {
                if (g != null) // could've been destroyed by the player already
                    Destroy(g);
            }
            _spawnedObjects[0].Clear();
            _listPool.ReturnToPool(_spawnedObjects[0]);
            _spawnedObjects.RemoveAt(0);
        }

    }

    private void AddTrash(TrackPiece trackPiece)
    {
        // Create or reuse a list to store the objects on this trackPiece
        List<GameObject> gameObjectsOnNewTrackPiece = _listPool.ProduceObject();
        _spawnedObjects.Add(gameObjectsOnNewTrackPiece);

        int numTrash = Random.Range(_minGarbageOnTrackPiece, _maxGarbageOnTrackPiece + 1);
        if (TrackPieces.Count < 3)
            numTrash = 0;

        foreach (GarbageSpawningBeatStrength g in _beatStrengths)
            g.StartNextTrackPiece();

        if (DevHelper.Instance.TrashCollectionTimingInfo.CheckTrashCollectionConsistentIntervals)
        {
            // Spawn trash pieces at every position.
            for (int i = 0; i < TrackPiece.TRACK_PIECE_LENGTH; i += TrackPiece.TRACK_PIECE_LENGTH / 16)
            {
                for (int j = -1; j <= 1; j++)
                {
                    Vector3 position = ChooseRandomPositionAndRotationForObjectOnTrack(trackPiece, out Quaternion rotation, distanceAlongMidline: i, lane: j);
                    GameObject instantiated = Instantiate(_trashPrefabForCheckingConsistentIntervals, position, rotation, transform);
                    gameObjectsOnNewTrackPiece.Add(instantiated);
                }
            }
        }
        else
        {
            for (int i = 0; i < numTrash; i++)
            {
                bool success = false;
                for (int j = 0; j < _beatStrengths.Length; j++)
                {
                    GarbageSpawningBeatStrength beatStrength = _beatStrengths[j];
                    beatStrength.Next(out bool allFull, out int beatToSpawnAt, out GameObject prefab);
                    if (!allFull)
                    {
                        Vector3 position = ChooseRandomPositionAndRotationForObjectOnTrack(trackPiece, out Quaternion rotation
                            , distanceAlongMidline: beatToSpawnAt * TrackPiece.TRACK_PIECE_LENGTH / 16, lane: Random.Range(-1, 2));
                        GameObject instantiated = Instantiate(prefab, position, rotation, transform);
                        gameObjectsOnNewTrackPiece.Add(instantiated);
                        success = true;
                        break;
                    }
                }
                if (!success)
                    throw new System.Exception("Failed to find a position to spawn. This is a bug or the inspector settings have more trash spawn than the number of beats.");
            }
        }

        
    }

    private Vector3 ChooseRandomPositionAndRotationForObjectOnTrack(TrackPiece trackPiece, out Quaternion rotation, float distanceAlongMidline, float lane)
    {
        float t = trackPiece.FindTForDistanceAlongMidline(distanceAlongMidline, 0f);


        // Convert positions on the midline to positions on the lane in the same way which the player movement decides where the player's position is.

        trackPiece.StoreLane(0);
        Vector3 midlinePosition = trackPiece.BezierCurve(t) + Vector3.up * _trackObjectsYOffset;

        Vector3 approximatedPositionOnMidline = trackPiece.BezierCurve(t);
        trackPiece.StoreLane(lane);
        Vector3 approximatedPositionAtLanePosition = trackPiece.BezierCurve(t);
        Vector3 offsetForLanePosition = approximatedPositionAtLanePosition - approximatedPositionOnMidline;

        Vector3 position = midlinePosition + offsetForLanePosition;
            

        Vector3 direction = trackPiece.BezierCurveDerivative(t);
        Vector3 directionOnPlane = new Vector3(direction.x, 0, direction.z);
        float directionAngle = Quaternion.FromToRotation(Vector3.forward, directionOnPlane).eulerAngles.y;
        rotation = Quaternion.Euler(0f, directionAngle, 0f);

           
        return position;
    }

    private void CreateFirstTrackPiece()
    {
        GameObject newTrackPiece = Instantiate(RandomTrackPiecePrefab(), transform);
        newTrackPiece.transform.position = Vector3.down * PlayerMovement.Settings.PlayerVerticalOffset;
        newTrackPiece.transform.rotation = Quaternion.identity;

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
