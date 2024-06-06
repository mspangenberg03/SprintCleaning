using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

// Save or load data to allow deterministic reproduction of gameplay which lead to a bug.

[DefaultExecutionOrder(-10000)]
public class DeterministicBugReproduction : MonoBehaviour
{
    private const string FILE_NAME = "Debug.json";

    [System.Serializable] 
    private class DebugSaveData
    {
        public int seed;
        public float fixedDeltaTime;
        public float[] targetLaneEachFixedUpdate;
    }

    //[SerializeField] private bool _reproduceBasedOnSaveData = false;

    public bool ReproduceBasedOnSaveData => DevSettings.Instance.ReproduceSavedRNGAndInputs;

    private DebugSaveData _saveData;
    private List<float> _targetLaneEachFixedUpdate = new();
    private int _fixedUpdateCount;
    private string _saveLocation;


    public static DeterministicBugReproduction Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        _saveLocation = Path.Combine(Application.persistentDataPath, FILE_NAME);
        Debug.Log("Bug reproduction file save location: " + _saveLocation);

        if (ReproduceBasedOnSaveData)
        {
            if (File.Exists(_saveLocation))
            {
                string loadPlayerData = File.ReadAllText(_saveLocation);
                _saveData = JsonUtility.FromJson<DebugSaveData>(loadPlayerData);
                _targetLaneEachFixedUpdate = new List<float>(_saveData.targetLaneEachFixedUpdate);
            }
            else
            {
                throw new System.Exception("The save file does not exist yet.");
            }
        }
        else
        {
            _saveData = new DebugSaveData();
            _saveData.seed = Random.Range(int.MinValue, int.MaxValue);
            PlayerMovementProcessor.SetFixedDeltaTime();
            _saveData.fixedDeltaTime = Time.fixedDeltaTime;
        }
        Time.fixedDeltaTime = _saveData.fixedDeltaTime;
        Random.InitState(_saveData.seed);
    }

    private void OnDestroy()
    {
        if (ReproduceBasedOnSaveData)
            return;
        // save the data
        _saveData.targetLaneEachFixedUpdate = _targetLaneEachFixedUpdate.ToArray();
        string data = JsonUtility.ToJson(_saveData);
        File.WriteAllText(_saveLocation, data);

        string loadPlayerData = File.ReadAllText(_saveLocation);
        _saveData = JsonUtility.FromJson<DebugSaveData>(loadPlayerData);
    }

    public bool OverrideTargetLane(out float targetLane)
    {
        if (!ReproduceBasedOnSaveData)
        {
            targetLane = 0;
            return false;
        }

        if (_fixedUpdateCount >= _targetLaneEachFixedUpdate.Count)
        {
            targetLane = 0;
        }
        else
        {
            targetLane = _targetLaneEachFixedUpdate[_fixedUpdateCount];
        }

        _fixedUpdateCount++;

        return true;
    }

    public void NextFixedUpdateTargetLane(float targetLane)
    {
        if (ReproduceBasedOnSaveData)
            return;
        _targetLaneEachFixedUpdate.Add(targetLane);
    }
}
