using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    [SerializeField]
    public ScoreManager _data;
    public Dictionary<GarbageType, string> _garbageConvert = new Dictionary<GarbageType, string>();
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
        _garbageConvert.Add(GarbageType.TrashCan, "Trash Can");
        _garbageConvert.Add(GarbageType.GlassShard, "Glass Shard");
        _garbageConvert.Add(GarbageType.Coke, "Coke");
        _garbageConvert.Add(GarbageType.Bottle, "Bottle");
        _garbageConvert.Add(GarbageType.MilkJug, "Milk Jug");
        _garbageConvert.Add(GarbageType.Banana, "Banana");
    }

}
