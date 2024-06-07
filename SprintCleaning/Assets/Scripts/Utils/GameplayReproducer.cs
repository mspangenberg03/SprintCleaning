using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

// Save or load data to allow deterministic reproduction of gameplay which lead to a bug.
public class GameplayReproducer
{
    private const string FILE_NAME = "Repro.json";
    private const string FILE_PREV_NAME = "Repro-prev.json";

    [System.Serializable] 
    private class DebugSaveData
    {
        public int seed;
        public float fixedDeltaTime;
        public float[] targetLaneEachFixedUpdate;
        public bool[] jumpEachFixedUpdate;
    }

    public bool _reproduceGameplay;

    private DebugSaveData _saveData;
    private List<float> _targetLaneEachFixedUpdate = new();
    private List<bool> _jumpEachFixedUpdate = new();
    private int _currentIndex = -1;
    private string _saveLocation;
    private string _savePrevLocation;

    public GameplayReproducer(bool reproduceGameplay)
    {
        _reproduceGameplay = reproduceGameplay;
        _saveLocation = Path.Combine(Application.persistentDataPath, FILE_NAME);
        _savePrevLocation = Path.Combine(Application.persistentDataPath, FILE_PREV_NAME);
        Debug.Log("Bug reproduction file: " + _saveLocation);

        if (_reproduceGameplay)
        {
            if (File.Exists(_saveLocation))
            {
                _saveData = JsonUtility.FromJson<DebugSaveData>(File.ReadAllText(_saveLocation));
                _targetLaneEachFixedUpdate = new List<float>(_saveData.targetLaneEachFixedUpdate);
                _jumpEachFixedUpdate = new List<bool>(_saveData.jumpEachFixedUpdate);
            }
            else
            {
                throw new System.Exception("The save file does not exist yet. Change the ReproduceGameplay setting on the DevSettings scriptable object to false.");
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

    public void CheckSave()
    {
        if (_reproduceGameplay)
            return;

        if (File.Exists(_saveLocation))
        {
            File.Delete(_savePrevLocation);
            File.Move(_saveLocation, _savePrevLocation);
        }

        // save the data
        _saveData.targetLaneEachFixedUpdate = _targetLaneEachFixedUpdate.ToArray();
        _saveData.jumpEachFixedUpdate = _jumpEachFixedUpdate.ToArray();

        File.WriteAllText(_saveLocation, JsonUtility.ToJson(_saveData));
    }

    public void StartNextFixedUpdate()
    {
        _currentIndex++;
    }

    public void SaveOrLoadTargetLane(ref float targetLane)
    {
        if (_reproduceGameplay)
        {
            if (_currentIndex < _targetLaneEachFixedUpdate.Count)
                targetLane = _targetLaneEachFixedUpdate[_currentIndex];
        }
        else
        {
            _targetLaneEachFixedUpdate.Add(targetLane);
        }
    }

    public void SaveOrLoadJump(ref bool jump)
    {
        if (_reproduceGameplay)
        {
            if (_currentIndex < _jumpEachFixedUpdate.Count)
                jump = _jumpEachFixedUpdate[_currentIndex];
        }
        else
        {
            _jumpEachFixedUpdate.Add(jump);
        }
    }
}
