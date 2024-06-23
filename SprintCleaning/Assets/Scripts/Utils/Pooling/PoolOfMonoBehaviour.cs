using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A pool of gameObjects with a particular script. The point is to avoid destroying gameObjects, to avoid garbage allocation for performance.
/// </summary>
/// <typeparam name="T">The type of the script on the prefab.</typeparam>
public class PoolOfMonoBehaviour<T> where T : MonoBehaviour, PoolOfMonoBehaviour<T>.IPoolable
{
    public static Garbage firstFail;

    public interface IPoolable
    {
        void InitializeUponPrefabInstantiated(PoolOfMonoBehaviour<T> pool);
        void InitializeUponProducedByPool();
        void OnReturnToPool();
    }


    private GameObject _prefab;
    private Transform _poolFolder;
    private Transform _outOfPoolFolder;
    private Stack<T> _pool = new();
    private Dictionary<T, GameObject> _rootOfEachPrefabInstance = new();


    public PoolOfMonoBehaviour(GameObject prefab, Transform poolFolder, Transform outOfPoolFolder)
    {
        _prefab = prefab;
        _poolFolder = poolFolder;
        _outOfPoolFolder = outOfPoolFolder;
    }

    /// <summary>
    /// Reuses or instantiates an instance of the prefab.
    /// </summary>
    /// <param name="position">The position to instantiate the prefab at.</param>
    /// <param name="rotation">The rotation to instantiate the prefab at.</param>
    /// <returns>A script on the prefab.</returns>
    public T Produce(Vector3 position, Quaternion rotation)
    {
        T result;
        if (_pool.Count == 0)
        {
            GameObject instantiated = Object.Instantiate(_prefab, position, rotation, _outOfPoolFolder);
            result = instantiated.GetComponentInChildren<T>(); // The script can be on the prefab's root gameObject.
            result.InitializeUponPrefabInstantiated(this);
            _rootOfEachPrefabInstance.Add(result, instantiated);
        }
        else
        {
            result = _pool.Pop();
#if UNITY_EDITOR
            if (_rootOfEachPrefabInstance[result].activeSelf)
                throw new System.Exception("_rootOfEachPrefabInstance[result].activeSelf");
#endif
            Transform rootTransform = _rootOfEachPrefabInstance[result].transform;
            rootTransform.parent = _outOfPoolFolder;
            rootTransform.position = position;
            rootTransform.rotation = rotation;
        }
        result.InitializeUponProducedByPool();
        GameObject rootGameObject = _rootOfEachPrefabInstance[result];
        rootGameObject.SetActive(true);
        return result;
    }

    /// <summary>
    /// Returns an instance of the prefab to the pool.
    /// </summary>
    /// <param name="toReturn">The script on the prefab to return.</param>
    public void ReturnToPool(T toReturn)
    {
#if UNITY_EDITOR
        if (toReturn == null)
            throw new System.ArgumentNullException("toReturn");
        if (_pool.Contains(toReturn))
            throw new System.InvalidOperationException("toReturn is already in the pool.");
#endif

        toReturn.OnReturnToPool();
        _rootOfEachPrefabInstance[toReturn].SetActive(false);
        Transform rootTransform = _rootOfEachPrefabInstance[toReturn].transform;
        rootTransform.parent = _poolFolder;
        _pool.Push(toReturn);
    }

    public bool InPool(T x) => _pool.Contains(x);
}
