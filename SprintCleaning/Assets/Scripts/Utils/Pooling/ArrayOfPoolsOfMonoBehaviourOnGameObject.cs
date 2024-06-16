using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrayOfPoolsOfMonoBehaviourOnGameObject<T> where T : MonoBehaviour, IPoolable
{
    private PoolOfMonoBehaviourOnGameObject<T>[] _pools;
    private Dictionary<T, PoolOfMonoBehaviourOnGameObject<T>> _poolOfEachInstance = new();

    public ArrayOfPoolsOfMonoBehaviourOnGameObject(GameObject[] prefabs, Transform instantiatedGameObjectsParent)
    {
        _pools = new PoolOfMonoBehaviourOnGameObject<T>[prefabs.Length];
        for (int i = 0; i < prefabs.Length; i++)
        {
            _pools[i] = new PoolOfMonoBehaviourOnGameObject<T>(prefabs[i], instantiatedGameObjectsParent);
        }
    }

    public T Produce(int prefabIndex, Vector3 position, Quaternion rotation, out Transform prefabInstanceRoot)
    {
        T result = _pools[prefabIndex].Produce(position, rotation, out prefabInstanceRoot);
        _poolOfEachInstance[result] = _pools[prefabIndex];
        return result;
    }

    public void ReturnToPool(T toReturn)
    {
        _poolOfEachInstance[toReturn].ReturnToPool(toReturn);
    }
}
