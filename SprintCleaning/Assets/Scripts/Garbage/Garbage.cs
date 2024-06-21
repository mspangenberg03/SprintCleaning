using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Garbage : MonoBehaviour, PoolOfMonoBehaviour<Garbage>.IPoolable
{
    [SerializeField] private GarbagePickupAnimationTrigger _animTrigger;

    [Tooltip("The type of tool needed to collect this piece of garbage")]
    public GarbageType _type;

    [Tooltip("The sound this piece of garbage plays on collect")]
    public AudioClip impact;

    [SerializeField] private GameObject _particle;

    [field: SerializeField] public bool Obstacle { get; private set; }

    public AudioSource _garbageAudio;

    [SerializeField,Tooltip("The score this garbage add")]
    private int _score = 1;

    [SerializeField, Tooltip("The value that collecting this garbage add to the current streak value")]
    private int _streakAddValue = 10;

    [field: SerializeField] public float Gravity { get; private set; }

    private float _horizontalSpeed;
    private float _verticalSpeed;
    private Vector3 _destinationPosition;
    private PoolOfMonoBehaviour<Garbage> _pool;
    public TrackPiece OnTrackPiece { get; set; }
    public static List<Garbage> ThrownGarbage { get; private set; } = new();

    private Transform Root => transform.parent;

    public void InitializeUponInstantiated(PoolOfMonoBehaviour<Garbage> pool)
    {
        _pool = pool;
    }
    public void InitializeUponProduced() 
    {
        _animTrigger.Reset();
    }
    public void OnReturnToPool() { }

    public void SetTrajectoryFromCurrentPosition(Vector3 destinationPosition)
    {
        if (destinationPosition.y > Root.position.y)
        {
            throw new System.InvalidOperationException("cannot fall upwards");
        }

        _destinationPosition = destinationPosition;
        float horizontalDistance = (destinationPosition.To2D() - Root.position.To2D()).magnitude;
        _verticalSpeed = 0;
        _horizontalSpeed = horizontalDistance / FallTime(Root.position, destinationPosition, Gravity);

        ThrownGarbage.Add(this);
    }

    public static float FallTime(Vector3 from, Vector3 destinationPosition, float gravity)
    {
        return Mathf.Sqrt(2 * (from.y - destinationPosition.y) / gravity);
    }

    public void MoveWhileBeingThrown()
    {
        Vector2 horizontalDirection = (_destinationPosition.To2D() - Root.position.To2D()).normalized;
        Vector3 velocity = (horizontalDirection * _horizontalSpeed).To3D() + Vector3.up * _verticalSpeed;

        Vector3 nextPosition = Root.position + velocity * Time.deltaTime;
        if (nextPosition.y < _destinationPosition.y)
        {
            ThrownGarbage.Remove(this);
            nextPosition = _destinationPosition;
        }
        Root.position = nextPosition;

        _verticalSpeed -= Gravity * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player")||other.gameObject.CompareTag("Vaccum"))
        {
            GameObject player = other.transform.parent.gameObject;
            Animator animator = player.GetComponentInChildren<Animator>();
            _garbageAudio = player.GetComponent<AudioSource>();
            _garbageAudio.PlayOneShot(impact, 1F);
            _animTrigger.CheckTriggerAnimation(other);
            if (!Obstacle)
            {
                GameObject particleObject = Instantiate(_particle, gameObject.transform.position, Quaternion.identity);
                particleObject.transform.rotation = player.transform.rotation;
            }

            if (DevHelper.Instance.LogUnexpectedTrashCollectionTimings)
            {
                // On my computer, the audio time updates every .02133 seconds. To be precisely synced with the music, it should be
                // at a .25 second interval
                double audioTime = GameplayMusic.CurrentAudioTime;
                if (audioTime % .25 > .022)
                    Debug.Log("Hit trash at time (+- maybe 20 ms): " + audioTime);
            }

            bool gameOver = false;
            if (Obstacle&&!other.gameObject.CompareTag("Vaccum"))
            {
                if (!Game_Over.Instance.GameIsOver)
                    animator.SetTrigger("Hit");
                player.GetComponent<Game_Over>().GameOver();
            }
            else{
                ScoreManager.Instance.GarbageCollected(_type);
                player.GetComponent<PlayerGarbageCollection>().TextEdit();
            }
            DevHelper.Instance.CheckLogInfoForTrashCollectionIntervalChecking();

            ScoreManager.Instance.AddScoreOnGarbageCollection(_score, _streakAddValue);

            if (!gameOver) // don't destroy it when game over because it looks strange how it disappears after a brief pause
                ReturnToPool();
#if UNITY_EDITOR
            if (!OnTrackPiece.GarbageOnThisTrackPiece.Contains(this))
                throw new System.Exception("not contained");
#endif
            OnTrackPiece.GarbageOnThisTrackPiece.Remove(this);
#if UNITY_EDITOR
            if (OnTrackPiece.GarbageOnThisTrackPiece.Contains(this))
                throw new System.Exception("jhtgfbvc");
#endif
        }
    }

    private void OnDestroy()
    {
        // This is only for when the scene unloads, because trash is destroyed at that time (b/c of pooling).
        // So it'll clear it a bunch of times when the scene unloads, because that's probably faster than removing this particular garbage from the list.
        ThrownGarbage.Clear();
    }

    public void ReturnToPool()
    {
#if UNITY_EDITOR
        if (InPool())
            throw new System.Exception("Already in pool");
#endif

        _pool.ReturnToPool(this);
    }

    public bool InPool() => _pool.InPool(this);
    
}
