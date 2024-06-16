using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DictionaryOfPoolsOfMonoBehaviour<T> where T : MonoBehaviour, PoolOfMonoBehaviour<T>.IPoolable
{
    private Dictionary<GameObject, PoolOfMonoBehaviour<T>> _poolOfEachPrefab = new();
    private Dictionary<T, PoolOfMonoBehaviour<T>> _poolOfEachInstance = new();
    private Transform _instantiatedGameObjectsParent;

    public DictionaryOfPoolsOfMonoBehaviour(Transform instantiatedGameObjectsParent)
    {
        _instantiatedGameObjectsParent = instantiatedGameObjectsParent;
    }

    public void CheckAddPoolForPrefab(GameObject prefab)
    {
        if (!_poolOfEachPrefab.ContainsKey(prefab))
            _poolOfEachPrefab.Add(prefab, new PoolOfMonoBehaviour<T>(prefab, _instantiatedGameObjectsParent));
    }

    public T Produce(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        PoolOfMonoBehaviour<T> pool = _poolOfEachPrefab[prefab];
        T result = pool.Produce(position, rotation);
        _poolOfEachInstance[result] = pool;
        return result;
    }

}
