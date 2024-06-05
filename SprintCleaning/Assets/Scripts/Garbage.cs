using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Garbage : MonoBehaviour
{
    [Tooltip("The type of tool needed to collect this piece of garbage")]
    public ToolType _type;

    [Tooltip("The sound this piece of garbage plays on collect")]
    public AudioClip impact;
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
            if (player.GetComponent<PlayerToolManager>().HasTool(_type))
            {
                _garbageAudio.PlayOneShot(impact, 1F);
                player.GetComponent<PlayerToolManager>().ToolUsed(_type);
                _playerItemData.GarbageCollected(_type);
                player.GetComponent<PlayerGarbageCollection>().TextEdit();
            }
            else
            {
                player.GetComponent<DirtinessManager>().AddDirtiness(_dirtiness);
                player.GetComponent<PlayerMovement>().GarbageSlow(_playerSpeedMultiplier);
            }
            Destroy(gameObject);
        }
    }
}
