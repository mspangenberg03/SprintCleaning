using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DataManager : MonoBehaviour
{
    [SerializeField]
    public ScoreManager _data;
    public Dictionary<GarbageType, string> _garbageConvert = new Dictionary<GarbageType, string>();

    public int _level {  get; private set; }

    public Dictionary<GarbageType, int> _counts => _data._counts;

    private static DataManager _instance;
    public static DataManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<DataManager>();
            }
            return _instance;
        }
    }

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        CreateGarbageConvert();
    }

    private void CreateGarbageConvert()
    {
        _garbageConvert.Add(GarbageType.TrashCan, "Trash Can");
        _garbageConvert.Add(GarbageType.GlassShard, "Glass Shard");
        _garbageConvert.Add(GarbageType.Coke, "Coke");
        _garbageConvert.Add(GarbageType.Bottle, "Bottle");
        _garbageConvert.Add(GarbageType.MilkJug, "Milk Jug");
        _garbageConvert.Add(GarbageType.Banana, "Banana");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (SceneManager.GetActiveScene().buildIndex <= Level_Tracker.Instance.LevelsUnlocked())
        {
            _data = ScoreManager.Instance;
        }
    }
}
