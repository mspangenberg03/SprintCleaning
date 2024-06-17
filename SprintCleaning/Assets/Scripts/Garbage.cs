using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    [SerializeField] private PlayerData _playerData;
    [SerializeField] private GameObject _particle;

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
                _playerData.GarbageCollected(_type);
                player.GetComponent<PlayerGarbageCollection>().TextEdit();
            }
            DevHelper.Instance.CheckLogInfoForTrashCollectionIntervalChecking();

            _garbageAudio.PlayOneShot(impact, 1F);
            _playerData.GarbageCollected(_type);
            player.GetComponent<PlayerGarbageCollection>().TextEdit();
            GameObject particleObject = Instantiate(_particle, gameObject.transform.position,Quaternion.identity);
            Quaternion forward = player.transform.rotation;
            particleObject.transform.rotation = forward;
            _playerData.AddScoreOnGarbageCollection(_score, _streakAddValue);
            

            Destroy(gameObject);
        }
    }
}
