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
        public bool[] leftArrowKeysEachFixedUpdate;
        public bool[] rightArrowKeysEachFixedUpdate;
    }

    [SerializeField] private bool _reproduceBasedOnSaveData = false;

    private DebugSaveData _saveData;
    private List<bool> _leftArrowKeysEachFixedUpdate = new();
    private List<bool> _rightArrowKeysEachFixedUpdate = new();
    private int _fixedUpdateCount;
    private string _saveLocation;


    public static DeterministicBugReproduction Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        _saveLocation = Path.Combine(Application.persistentDataPath, FILE_NAME);
        Debug.Log("Save location: " + _saveLocation);

        if (_reproduceBasedOnSaveData)
        {
            if (File.Exists(_saveLocation))
            {
                string loadPlayerData = File.ReadAllText(_saveLocation);
                _saveData = JsonUtility.FromJson<DebugSaveData>(loadPlayerData);
                _leftArrowKeysEachFixedUpdate = new List<bool>(_saveData.leftArrowKeysEachFixedUpdate);
                _rightArrowKeysEachFixedUpdate = new List<bool>(_saveData.rightArrowKeysEachFixedUpdate);
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
        if (_reproduceBasedOnSaveData)
            return;
        // save the data
        _saveData.leftArrowKeysEachFixedUpdate = _leftArrowKeysEachFixedUpdate.ToArray();
        _saveData.rightArrowKeysEachFixedUpdate = _rightArrowKeysEachFixedUpdate.ToArray();
        string data = JsonUtility.ToJson(_saveData);
        File.WriteAllText(_saveLocation, data);

        string loadPlayerData = File.ReadAllText(_saveLocation);
        _saveData = JsonUtility.FromJson<DebugSaveData>(loadPlayerData);
    }

    public bool OverrideControl(out bool leftKey, out bool rightKey)
    {
        if (!_reproduceBasedOnSaveData)
        {
            leftKey = false;
            rightKey = false;
            return false;
        }

        if (_fixedUpdateCount >= _leftArrowKeysEachFixedUpdate.Count)
        {
            leftKey = false;
            rightKey = false;
        }
        else
        {
            leftKey = _leftArrowKeysEachFixedUpdate[_fixedUpdateCount];
            rightKey = _rightArrowKeysEachFixedUpdate[_fixedUpdateCount];
        }

        _fixedUpdateCount++;

        return true;
    }

    public void NextFixedUpdateInputs(bool leftKey, bool rightKey)
    {
        if (_reproduceBasedOnSaveData)
            return;
        _leftArrowKeysEachFixedUpdate.Add(leftKey);
        _rightArrowKeysEachFixedUpdate.Add(rightKey);
    }
}
