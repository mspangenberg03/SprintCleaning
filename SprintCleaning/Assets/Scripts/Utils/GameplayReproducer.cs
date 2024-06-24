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
        public bool[] leftInputDowns;
        public bool[] rightInputDowns;
        public bool[] jumps;
    }

    public bool _reproduceGameplay;
    public bool _dontOverrideControls;

    private DebugSaveData _saveData;

    private List<bool> _leftInputDowns = new();
    private List<bool> _rightInputDowns = new();
    private List<bool> _jumpInputs = new();
    private int _currentIndex = -1;
    private string _saveLocation;
    private string _savePrevLocation;

    public GameplayReproducer(bool reproduceGameplay, bool dontOverrideControls)
    {
        _reproduceGameplay = reproduceGameplay;
        _dontOverrideControls = dontOverrideControls;
        _saveLocation = Path.Combine(Application.persistentDataPath, FILE_NAME);
        _savePrevLocation = Path.Combine(Application.persistentDataPath, FILE_PREV_NAME);
        Debug.Log("Bug reproduction file: " + _saveLocation);

        if (_reproduceGameplay)
        {
            if (File.Exists(_saveLocation))
            {
                _saveData = JsonUtility.FromJson<DebugSaveData>(File.ReadAllText(_saveLocation));
                _leftInputDowns = new List<bool>(_saveData.leftInputDowns);
                _rightInputDowns = new List<bool>(_saveData.rightInputDowns);
                _jumpInputs = new List<bool>(_saveData.jumps);
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
        _saveData.leftInputDowns = _leftInputDowns.ToArray();
        _saveData.rightInputDowns = _rightInputDowns.ToArray();
        _saveData.jumps = _jumpInputs.ToArray();

        File.WriteAllText(_saveLocation, JsonUtility.ToJson(_saveData));
    }

    public void StartNextFixedUpdate()
    {
        _currentIndex++;
    }

    public void SaveOrLoadMovementInputs(ref bool leftInputDown, ref bool rightInputDown, ref bool jumpInput)
    {
        SaveOrLoadBoolInput(ref leftInputDown, _leftInputDowns);
        SaveOrLoadBoolInput(ref rightInputDown, _rightInputDowns);
        SaveOrLoadBoolInput(ref jumpInput, _jumpInputs);
    }

    private void SaveOrLoadBoolInput(ref bool input, List<bool> inputs)
    {
        if (_reproduceGameplay)
        {
            if (!_dontOverrideControls)
            {
                if (_currentIndex < inputs.Count)
                    input = inputs[_currentIndex];
            }
        }
        else
            inputs.Add(input);
    }
}
