using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrayOfPoolsOfMonoBehaviour<T> where T : MonoBehaviour, PoolOfMonoBehaviour<T>.IPoolable
{
    private PoolOfMonoBehaviour<T>[] _pools;
    private Dictionary<T, PoolOfMonoBehaviour<T>> _poolOfEachInstance = new();

    public ArrayOfPoolsOfMonoBehaviour(GameObject[] prefabs, Transform poolFolder, Transform outOfPoolFolder)
    {
        _pools = new PoolOfMonoBehaviour<T>[prefabs.Length];
        for (int i = 0; i < prefabs.Length; i++)
        {
            _pools[i] = new PoolOfMonoBehaviour<T>(prefabs[i], poolFolder, outOfPoolFolder);
        }
    }

    public T Produce(int prefabIndex, Vector3 position, Quaternion rotation)
    {
        PoolOfMonoBehaviour<T> pool = _pools[prefabIndex];
        T result = pool.Produce(position, rotation);
        _poolOfEachInstance[result] = pool;
        return result;
    }

    public void ReturnToPool(T toReturn)
    {
        _poolOfEachInstance[toReturn].ReturnToPool(toReturn);
    }
}
