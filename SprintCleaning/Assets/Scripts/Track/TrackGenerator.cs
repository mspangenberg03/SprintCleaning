using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TrackGenerator : MonoBehaviour
{
    private const float STANDARD_TRACK_PIECE_LENGTH = 16f;

    [Header("Track")]
    
    [Tooltip("Amount of empty spawns, 0 is easiest, 5(placeholder) is no empty spawns")]
    [SerializeField] private int _diff = 0; 
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
    [SerializeField] private int _minGarbageOnTrackPiece = 4;
    [SerializeField] private int _maxGarbageOnTrackPiece = 16;
    [Header("Beat strengths filled from 1st to last.")]
    [SerializeField] private GarbageSpawningBeatStrength[] _beatStrengths;

    private GameObject _trashPrefabForCheckingConsistentIntervals;
    private int _totalTrackPieces;
    private int _priorTrackPieceIndex;
    private int _numStraightSinceLastTurn;
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
        _trashPrefabForCheckingConsistentIntervals = _beatStrengths[0].GarbagePrefabs[0];
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
        AddTrash(newTrackPiece);

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

    private void AddTrash(TrackPiece trackPiece)
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

        int numTrash = Random.Range(_minGarbageOnTrackPiece, _maxGarbageOnTrackPiece + 1);
        if (_totalTrackPieces < 5)
            numTrash = 0;

        foreach (GarbageSpawningBeatStrength g in _beatStrengths)
            g.StartNextTrackPiece();

        if (DevHelper.Instance.TrashCollectionTimingInfo.CheckTrashCollectionConsistentIntervals)
        {
            // Spawn trash pieces at every position.
            for (int i = 0; i < TrackPiece.TRACK_PIECE_LENGTH; i += 2)
            {
                for (int j = -1; j <= 1; j++)
                {
                    Vector3 position = ChooseRandomPositionAndRotationForObjectOnTrack(trackPiece, out Quaternion rotation, forceDistanceAlongMidline: i, forceLane: j);
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
                        Vector3 position = ChooseRandomPositionAndRotationForObjectOnTrack(trackPiece, out Quaternion rotation, forceDistanceAlongMidline: beatToSpawnAt, forceLane: Random.Range(-1, 2));
                        GameObject instantiated = Instantiate(prefab, position, rotation, transform);
                        gameObjectsOnNewTrackPiece.Add(instantiated);
                        success = true;
                        break;
                    }
                }
                if (!success)
                    throw new System.Exception("Failed to find a position to spawn. This is a bug or the inspector settings have more trash spawn than the number of beats.");
            }



            //GameObject prefab;
            //for (int i = 0; i < TrackPiece.TRACK_PIECE_LENGTH; i += 2)
            //{
            //    if(i == 0 || i ==8){
            //        prefab = _trashPrefabs[0];
            //    }
            //    else if(i== 4 || i == 12){
            //        prefab = _trashPrefabs[1];
            //    }
            //    else{
            //        prefab = _trashPrefabs[2];
            //    }
            //    Vector3 position = ChooseRandomPositionAndRotationForObjectOnTrack(trackPiece, out Quaternion rotation, forceDistanceAlongMidline: i);
            
            //    GameObject instantiated = Instantiate(prefab, position, rotation, transform);
            //    gameObjectsOnNewTrackPiece.Add(instantiated);
            //}
            
            /*
            // Add trash pieces
            //i += x where x is interval between spawns
            int[] poslist = [-1,0,1];
            for (int i = 0; i < TrackPiece.TRACK_PIECE_LENGTH; i += 1)
            {
                
                GameObject prefab = _trashPrefabs[Random.Range(0, (_trashPrefabs.Length - _diff))]; // Could do a random bag to prevent too many of the same type of trash
                Vector3 position = ChooseRandomPositionAndRotationForObjectOnTrack(trackPiece, out Quaternion rotation, forceDistanceAlongMidline: i, forceLane: j);
                if (float.IsNaN(position.x))
                    break; // Couldn't find a valid position
                        //Quaternion rotation = Quaternion.Euler(0, Random.Range(-180f, 180f), 0f);
                GameObject instantiated = Instantiate(prefab, position, rotation, transform);
                gameObjectsOnNewTrackPiece.Add(instantiated);
                
            }
            */
        }

        
    }

    private Vector3 ChooseRandomPositionAndRotationForObjectOnTrack(TrackPiece trackPiece, out Quaternion rotation, float forceDistanceAlongMidline = -1f, float forceLane = -2f)
    {
        int attemptsLeft = 100;
        while (true)
        {
            attemptsLeft--;
            if (attemptsLeft == 0)
                break;

            float lane = Random.Range(-1, 2);

            if (forceLane != -2f)
                lane = forceLane;

            float distanceAlongMidline = Random.Range(0, TrackPiece.TRACK_PIECE_LENGTH);
            if (forceDistanceAlongMidline != -1f)
                distanceAlongMidline = forceDistanceAlongMidline;
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

            if (!invalid || forceDistanceAlongMidline != -1f)
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
