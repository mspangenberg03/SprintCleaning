using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DataManager : MonoBehaviour
{
    public Dictionary<GarbageType, string> _garbageConvert = new Dictionary<GarbageType, string>();

    public int _level {  get; private set; }

    private static DataManager _instance;
    public static DataManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<DataManager>();

                if (_instance == null)
                {
                    GameObject instantiated = Instantiate(Resources.Load<GameObject>("DataManager"));
                    _instance = instantiated.GetComponent<DataManager>();
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null)
            Destroy(gameObject);
        else
        {
            DontDestroyOnLoad(transform.gameObject);
            CreateGarbageConvert();
        }
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
}
