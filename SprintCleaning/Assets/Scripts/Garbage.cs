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

    private static float _lastTime;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GameObject player = other.transform.parent.gameObject;
            _garbageAudio = player.GetComponent<AudioSource>();

            if (DevSettings.Instance.CheckTrashCollectionConsistentIntervals)
            {
                int fixedTimesteps = Mathf.RoundToInt((Time.fixedTime - _lastTime) / Time.fixedDeltaTime);
                bool expected = false;
                foreach (int x in DevSettings.Instance.ExpectedFixedUpdatesBetweenTrashCollection)
                {
                    if (x == fixedTimesteps)
                        expected = true;
                }
                if (!expected)
                    Debug.Log("Unexpected number of fixed timesteps between Garbage.OnTriggerEnter: " + fixedTimesteps);
                _lastTime = Time.fixedTime;
            }
            _garbageAudio.PlayOneShot(impact, 1F);
                //player.GetComponent<PlayerToolManager>().ToolUsed(_type);
                _playerItemData.GarbageCollected(_type);
                player.GetComponent<PlayerGarbageCollection>().TextEdit();

            Destroy(gameObject);
        }
    }
}
