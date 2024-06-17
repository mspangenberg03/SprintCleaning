using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Garbage : MonoBehaviour
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GameObject player = other.transform.parent.gameObject;
            _garbageAudio = player.GetComponent<AudioSource>();
            _garbageAudio.PlayOneShot(impact, 1F);
            if (_obstacle)
            {
                player.GetComponent<Game_Over>().GameOver();
            }
            else{
                ScoreManager.Instance.GarbageCollected(_type);
                player.GetComponent<PlayerGarbageCollection>().TextEdit();
            }
            DevHelper.Instance.CheckLogInfoForTrashCollectionIntervalChecking();

            _garbageAudio.PlayOneShot(impact, 1F);
            ScoreManager.Instance.GarbageCollected(_type);
            player.GetComponent<PlayerGarbageCollection>().TextEdit();
            ScoreManager.Instance.AddScoreOnGarbageCollection(_score, _streakAddValue);
            

            Destroy(gameObject);
        }
    }
}
