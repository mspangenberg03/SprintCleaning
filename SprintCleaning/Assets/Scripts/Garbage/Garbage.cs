using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Garbage : MonoBehaviour, PoolOfMonoBehaviour<Garbage>.IPoolable
{
    [SerializeField] private GarbagePickupAnimationTrigger _animTrigger;

    [Tooltip("The type of tool needed to collect this piece of garbage")]
    [SerializeField] private GarbageType _type;

    [Tooltip("The sound this piece of garbage plays on collect")]
    [SerializeField] private AudioClip impact;

    [SerializeField] private GameObject _pickupParticlesPrefab;


    [field: SerializeField] public bool Obstacle { get; private set; }
    [field: SerializeField] public bool Powerup { get; private set; }


    [SerializeField,Tooltip("The score this garbage add")]
    private int _score = 1;

    [SerializeField, Tooltip("The value that collecting this garbage add to the current streak value")]
    private int _streakAddValue = 10;

    [field: SerializeField] public float Gravity { get; private set; }

    [SerializeField] private ParticleSystem _throwTrailParticleSystem;
    [SerializeField] private float _particleDisableTimer;

    private ParticleSystem.MainModule _throwParticleSystemMainModule;
    private float _disableParticlesAtTime = float.PositiveInfinity;
    private bool _onGroundButInThrownGarbageListToDisableParticles;
    private float _horizontalSpeed;
    private float _verticalSpeed;
    private Vector3 _destinationPosition;
    private PoolOfMonoBehaviour<Garbage> _pool;

    private static double _syncDebug_lastTime;
    private static double _syncDebug_totalIntervals;
    private static int _syncDebug_count;
    private static int _syncDebug_loopCount;



    public TrackPiece OnTrackPiece { get; set; }
    public static List<Garbage> ThrownGarbage { get; private set; } = new();

    private Transform Root => transform.parent;

    public int DebugID { get; set; }

    public void InitializeUponPrefabInstantiated(PoolOfMonoBehaviour<Garbage> pool)
    {
        _pool = pool;
        _throwParticleSystemMainModule = _throwTrailParticleSystem.main;
    }
    public void InitializeUponProducedByPool() 
    {
        if (_animTrigger != null)
            _animTrigger.Reset();
    }
    public void OnReturnToPool() 
    {
        ThrownGarbage.Remove(this);
        _throwTrailParticleSystem.gameObject.SetActive(false);
        _disableParticlesAtTime = float.PositiveInfinity;
        _onGroundButInThrownGarbageListToDisableParticles = false;
    }

    public void StartBeingThrownTo(Vector3 destinationPosition)
    {
        _destinationPosition = destinationPosition;
        float horizontalDistance = (destinationPosition.To2D() - Root.position.To2D()).magnitude;
        _verticalSpeed = 0;
        _horizontalSpeed = horizontalDistance / FallTime(Root.position, destinationPosition, Gravity);

        ThrownGarbage.Add(this);
        _throwTrailParticleSystem.gameObject.SetActive(true);
    }

    public static float FallTime(Vector3 from, Vector3 destinationPosition, float gravity)
    {
        return Mathf.Sqrt(2 * (from.y - destinationPosition.y) / gravity);
    }

    public void MoveWhileBeingThrown(bool preventEnd = false)
    {
        if (_onGroundButInThrownGarbageListToDisableParticles)
        {
            if (Time.time >= _disableParticlesAtTime)
            {
                ThrownGarbage.Remove(this);
                _throwTrailParticleSystem.gameObject.SetActive(false);
            }
            return;
        }


        Vector2 horizontalDirection = (_destinationPosition.To2D() - Root.position.To2D()).normalized;
        Vector3 velocity = (horizontalDirection * _horizontalSpeed).To3D() + Vector3.up * _verticalSpeed;
        _throwParticleSystemMainModule.emitterVelocity = velocity;

        Vector3 nextPosition = Root.position + velocity * Time.deltaTime;
        if (nextPosition.y < _destinationPosition.y && !preventEnd) // dunno whether the preventEnd is necessary
        {
            _onGroundButInThrownGarbageListToDisableParticles = true;
            _disableParticlesAtTime = Time.time + _particleDisableTimer;
            nextPosition = _destinationPosition;
        }
        Root.position = nextPosition;

        _verticalSpeed -= Gravity * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player") && !other.gameObject.CompareTag("Vaccum"))
            return;

        if (Powerup)
            return;

        if (Game_Over.Instance.GameIsOver || Game_Over.Instance.LevelIsComplete)
            return;

        GameObject player = other.transform.parent.gameObject;
        Animator animator = player.GetComponentInChildren<Animator>();
        player.GetComponent<AudioSource>().PlayOneShot(impact, 1F);
        _animTrigger.CheckTriggerAnimation(other);
        if (!Obstacle)
        {
            GameObject particleObject = Instantiate(_pickupParticlesPrefab, gameObject.transform.position, Quaternion.identity);
            particleObject.transform.rotation = player.transform.rotation;
        }

        if (DevHelper.Instance.LogUnexpectedTrashCollectionTimings)
        {
            // On my computer, the audio time updates every .02133 seconds. To be precisely synced with the music, it should be
            // at a .25 second interval (or different depending on the level)
            System.Threading.Thread.MemoryBarrier();
            double audioTime = GameplayMusic.CurrentAudioTime;
            const double interval = .25 * 120 / 144;
            if (audioTime - _syncDebug_lastTime > 0)
            {
                _syncDebug_totalIntervals += audioTime - _syncDebug_lastTime;
                _syncDebug_count++;
            }
            else if (audioTime != 0)
                _syncDebug_loopCount++;
            double remainder = audioTime % interval;
            remainder = System.Math.Min(remainder, interval - remainder);
            if (remainder > .036)
            {
                Debug.Log("Hit trash at time (+- maybe 20 ms): " + audioTime
                    + ", remainder (assumes a particular level in the interval const): " + remainder 
                    + ", time since last time: " + (audioTime - _syncDebug_lastTime) 
                    + ", average interval: " + _syncDebug_totalIntervals / _syncDebug_count 
                    + " total intervals: " + _syncDebug_totalIntervals 
                    + " count: " + _syncDebug_count
                    + ", loop count: " + _syncDebug_loopCount);
            }
            _syncDebug_lastTime = audioTime;
        }

        bool gameOver = false;
        if (Obstacle&&!other.gameObject.CompareTag("Vaccum"))
        {
            if (!Game_Over.Instance.GameIsOver)
                animator.SetTrigger("Hit");
            player.GetComponent<Game_Over>().GameOver();
        }
        else
        {
            ScoreManager.Instance.GarbageCollected(_type);
            ScoreManager.Instance.AddScoreOnGarbageCollection(_score, _streakAddValue);
        }

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
        ThrownGarbage.Remove(this);

        _pool.ReturnToPool(this);
    }

    public bool InPool() => _pool.InPool(this);
    
}
