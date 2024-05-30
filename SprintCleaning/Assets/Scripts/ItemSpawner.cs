using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ItemSpawner : MonoBehaviour
{
    List<Vector3> _usedPoints = new List<Vector3>();
    int _numberOfItems;

    [SerializeField,Tooltip("The max amount of items that can spawn on a track piece")]
    private int _maxItems;

    [SerializeField]
    private GameObject _itemToSpawn;

    [SerializeField,Tooltip("The types of garbage that can spawn")]
    private List<GameObject> _garbageTypes = new List<GameObject>();
    [SerializeField, Tooltip("")]
    private List<GameObject> _toolTypes = new List<GameObject>();

    private const int _chanceOfSpawningTool = 40; 
    

    public void SpawnItems(GameObject trackPiece)
    {
        _numberOfItems = Random.Range(1,_maxItems);
        for(int i = 0; i < _numberOfItems; i++)
        {
            Vector3 _itemSpawn = new Vector3();
            while(_usedPoints.Contains(_itemSpawn))
            {
                _itemSpawn = trackPiece.transform.TransformPoint(new Vector3(Random.Range(-.5f,.5f),1f, Random.Range(-.5f, .5f)));
            }
            _usedPoints.Add(_itemSpawn);
            Instantiate(PickItem(), _itemSpawn, Quaternion.identity,null);
        }
    }
    private GameObject PickItem()
    {
        int rng = Random.Range(0, 100);
        switch (rng)
        {
            case < _chanceOfSpawningTool:
                return _toolTypes[Random.Range(0, _toolTypes.Count)];
            case >= _chanceOfSpawningTool:
                return _garbageTypes[Random.Range(0, _garbageTypes.Count)];
        }
    }
}
