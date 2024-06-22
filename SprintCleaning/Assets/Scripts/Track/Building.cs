using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour, PoolOfMonoBehaviour<Building>.IPoolable
{
    private const float THROW_RADIUS = 10f;

    [SerializeField] private Transform _earliestCornerByTrack;
    [SerializeField] private Transform _latestCornerByTrack;
    [SerializeField] private Transform[] _throwSources;

    private PoolOfMonoBehaviour<Building> _pool;
    private static List<Transform> _allThrowSources = new();
    private static List<Transform> _throwSourceCandidates = new();

    public float Width => (_earliestCornerByTrack.position - _latestCornerByTrack.position).magnitude;

    public void InitializeUponInstantiated(PoolOfMonoBehaviour<Building> pool) 
    {
        _pool = pool;
    }

    // to do (need to do VectorUtils.PolygonsOverlap and provide the footprint of each building)
    //public static bool Overlapping(Building a, Building b)
    //{

    //}

    public void InitializeUponProduced() 
    {
        foreach (Transform t in _throwSources)
            _allThrowSources.Add(t);
    }

    public void OnReturnToPool() 
    {
        foreach (Transform t in _throwSources)
            _allThrowSources.Remove(t);
    }

    public void ReturnToPool() => _pool.ReturnToPool(this);

    private void OnDestroy()
    {
        _allThrowSources.Clear();
    }

    public static Vector3 GetInitialPositionForThrownTrash(Vector3 destinationPosition)
    {
        _throwSourceCandidates.Clear();
        Transform closest = null;
        float closestSqrDistance = float.PositiveInfinity;
        for (int i = 0; i < _allThrowSources.Count; i++)
        {
            Transform next = _allThrowSources[i];
            float sqrDistance = (destinationPosition - next.position).To2D().sqrMagnitude;
            if (sqrDistance < closestSqrDistance)
            {
                closest = next;
                closestSqrDistance = sqrDistance;
            }
            if (sqrDistance < THROW_RADIUS * THROW_RADIUS)
                _throwSourceCandidates.Add(next);
        }

        if (_throwSourceCandidates.Count == 0)
            return closest.position;
        return _throwSourceCandidates[Random.Range(0, _throwSourceCandidates.Count)].position;
    }
}
