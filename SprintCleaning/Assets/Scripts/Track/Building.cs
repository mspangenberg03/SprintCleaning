using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour, PoolOfMonoBehaviour<Building>.IPoolable
{
    [SerializeField] private Transform _earliestCornerByTrack;
    [SerializeField] private Transform _latestCornerByTrack;
    [SerializeField] private Transform _earliestCornerOnFarSide;
    [SerializeField] private Transform _latestCornerOnFarSide;


    private PoolOfMonoBehaviour<Building> _pool;

    public float Width => (_earliestCornerByTrack.position - _latestCornerByTrack.position).magnitude;

    public void InitializeUponInstantiated(PoolOfMonoBehaviour<Building> pool) 
    {
        _pool = pool;
    }

    // to do (need to do VectorUtils.PolygonsOverlap and provide the footprint of each building)
    //public static bool Overlapping(Building a, Building b)
    //{

    //}

    public void InitializeUponProduced() { }

    public void OnReturnToPool() { }

    public void ReturnToPool() => _pool.ReturnToPool(this);
}
