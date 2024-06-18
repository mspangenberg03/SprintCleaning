using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DevHelper", menuName = "DevHelper")]
public class DevHelper : ScriptableObject
{
    [SerializeField] private bool _reproduceGameplay;
    [SerializeField] private bool _dontOverrideControlsWhenReproduceGameplay;
    [field: SerializeField] public bool LogAudioTimeAndPlayerProgressAlongTrack { get; private set; }
    [field: SerializeField] public bool LogUnexpectedTrashCollectionTimings { get; private set; }
    [field: SerializeField] public TrashCollectionTimingInfoSettings TrashCollectionTimingInfo { get; private set; }


    [System.Serializable]
    public class TrashCollectionTimingInfoSettings
    {
        [field: Header("When true, logs the number of fixed updates between trash collection, excluding those in the array.")]
        [field: SerializeField] public bool CheckTrashCollectionConsistentIntervals { get; private set; }
        [field: SerializeField] public bool DontLogIntervals { get; private set; }
        [field: SerializeField] public int[] DontLogFixedUpdatesBetweenTrashCollection = new int[] { 28, 29 };
    }

    private static DevHelperRef _ref;
    public static DevHelper Instance
    {
        get
        {
            if (_ref == null)
            {
                _ref = FindObjectOfType<DevHelperRef>();
                if (_ref == null)
                {
                    GameObject prefab = Resources.Load<GameObject>("Dev Helper Ref");
                    Instantiate(prefab);
                    _ref = prefab.GetComponent<DevHelperRef>();
                }
                _ref.SO.GameplayReproducer = new GameplayReproducer(_ref.SO._reproduceGameplay, _ref.SO._dontOverrideControlsWhenReproduceGameplay);
            }
            return _ref.SO;
        }
    }


    private float _lastTrashCollectionTime;

    public GameplayReproducer GameplayReproducer { get; private set; }

    public void OnDestroyRef()
    {
        GameplayReproducer.CheckSave();
        _lastTrashCollectionTime = -1;
    }

    public void CheckLogInfoForTrashCollectionIntervalChecking()
    {
        if (!TrashCollectionTimingInfo.CheckTrashCollectionConsistentIntervals)
            return;
        
        int fixedTimesteps = Mathf.RoundToInt((Time.fixedTime - _lastTrashCollectionTime) / Time.fixedDeltaTime);
        bool expected = false;
        foreach (int x in TrashCollectionTimingInfo.DontLogFixedUpdatesBetweenTrashCollection)
        {
            if (x == fixedTimesteps)
                expected = true;
        }
        if (!TrashCollectionTimingInfo.DontLogIntervals && !expected && _lastTrashCollectionTime != -1)
            Debug.Log("Number of fixed timesteps between Garbage.OnTriggerEnter: " + fixedTimesteps);
        _lastTrashCollectionTime = Time.fixedTime;
    }
}
