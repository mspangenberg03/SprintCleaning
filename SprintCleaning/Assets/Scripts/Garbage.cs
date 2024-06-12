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

    [SerializeField,Tooltip("How much dirtiness does this garbage add")]
    private int _dirtiness = 10;

    [SerializeField, Tooltip("Multiplies the player's speed")]
    private float _playerSpeedMultiplier = .3f;

    [SerializeField] private CollectedItems _playerItemData;

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
                _playerItemData.GarbageCollected(_type);
                player.GetComponent<PlayerGarbageCollection>().TextEdit();
            }
            DevHelper.Instance.CheckLogInfoForTrashCollectionIntervalChecking();

            

            Destroy(gameObject);
        }
    }
}
