using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Garbage : MonoBehaviour, PoolOfMonoBehaviour<Garbage>.IPoolable
{
    [Tooltip("The type of tool needed to collect this piece of garbage")]
    public GarbageType _type;

    [Tooltip("The sound this piece of garbage plays on collect")]
    public AudioClip impact;

    [SerializeField] private bool _obstacle;
    public AudioSource _garbageAudio;

    [SerializeField,Tooltip("The score this garbage add")]
    private int _score = 1;

    [SerializeField, Tooltip("The value that collecting this garbage add to the current streak value")]
    private int _streakAddValue = 10;

    [SerializeField] private PlayerData _playerData;

    private PoolOfMonoBehaviour<Garbage> _pool;
    private TrackPiece _onTrackPiece;

    public void InitializeUponInstantiated(PoolOfMonoBehaviour<Garbage> pool)
    {
        _pool = pool;
    }
    public void InitializeUponProduced() 
    {
        _onTrackPiece = TrackObjectsGenerator.TrackPieceGeneratingObjectsOn;
    }

    public void OnReturnToPool() { }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GameObject player = other.transform.parent.gameObject;
            _garbageAudio = player.GetComponent<AudioSource>();
            _garbageAudio.PlayOneShot(impact, 1F);

            if (DevHelper.Instance.LogUnexpectedTrashCollectionTimings)
            {
                // On my computer, the audio time updates every .02133 seconds. To be precisely synced with the music, it should be
                // at a .25 second interval
                double audioTime = GameplayMusic.CurrentAudioTime;
                if (audioTime % .25 > .022)
                    Debug.Log("Hit trash at time (+- maybe 20 ms): " + audioTime);
            }

            if (_obstacle)
            {
                player.GetComponent<Game_Over>().GameOver();
            }
            else{
                _playerData.GarbageCollected(_type);
                player.GetComponent<PlayerGarbageCollection>().TextEdit();
            }
            DevHelper.Instance.CheckLogInfoForTrashCollectionIntervalChecking();

            _playerData.AddScoreOnGarbageCollection(_score, _streakAddValue);


            ReturnToPool();
            _onTrackPiece.GarbageOnThisTrackPiece.Remove(this);
        }
    }

    public void ReturnToPool() => _pool.ReturnToPool(this);
    
}
