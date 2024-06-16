using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A pool of gameObjects with a particular script. Compared to instantiating and destroying 
/// </summary>
/// <typeparam name="T">The type of the script on the prefab.</typeparam>
public class PoolOfMonoBehaviourOnGameObject<T> where T : MonoBehaviour, IPoolable
{
    private GameObject _prefab;
    private Transform _instantiatedGameObjectsParent;
    private Stack<T> _pool = new();
    private Dictionary<T, GameObject> _rootOfEachPrefabInstance = new();


    public PoolOfMonoBehaviourOnGameObject(GameObject prefab, Transform instantiatedGameObjectsParent)
    {
        _prefab = prefab;
        _instantiatedGameObjectsParent = instantiatedGameObjectsParent;
    }


    /// <summary>
    /// Reuses or instantiates an instance of the prefab.
    /// </summary>
    /// <param name="position">The position to instantiate the prefab at.</param>
    /// <param name="rotation">The rotation to instantiate the prefab at.</param>
    /// <returns>A script on the prefab.</returns>
    public T Produce(Vector3 position, Quaternion rotation, out Transform prefabInstanceRoot)
    {
        T result;
        if (_pool.Count == 0)
        {
            GameObject instantiated = Object.Instantiate(_prefab, position, rotation, _instantiatedGameObjectsParent);
            result = instantiated.GetComponentInChildren<T>(); // The script can be on the prefab's root gameObject.
            _rootOfEachPrefabInstance.Add(result, instantiated);
        }
        else
            result = _pool.Pop();
        GameObject prefabInstanceRootGameObject = _rootOfEachPrefabInstance[result];
        prefabInstanceRootGameObject.SetActive(true);
        prefabInstanceRoot = prefabInstanceRootGameObject.transform;
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

        toReturn.ResetUponReturnToPool();
        _rootOfEachPrefabInstance[toReturn].SetActive(false);
        _pool.Push(toReturn);
    }
}
