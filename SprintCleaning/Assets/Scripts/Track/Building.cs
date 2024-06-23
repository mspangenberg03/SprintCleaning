using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour, PoolOfMonoBehaviour<Building>.IPoolable
{
    private const float THROW_RADIUS = 10f;

    [SerializeField] private Transform _earliestCornerByTrack;
    [SerializeField] private Transform _latestCornerByTrack;
    [SerializeField] private Transform[] _throwSources;
    [Header("Each footprint part must be a convex polygon, with the points in clockwise order.")]
    [SerializeField] private Transform[] _footprintPart1;
    [SerializeField] private Transform[] _footprintPart2;
    [SerializeField] private Transform[] _footprintPart3;
    [SerializeField] private Transform[] _footprintPart4;

    private Vector2[][] _footprint;

    private PoolOfMonoBehaviour<Building> _pool;
    private static List<Transform> _allThrowSources = new();
    private static List<Transform> _throwSourceCandidates = new();

    public float Width => (_earliestCornerByTrack.position - _latestCornerByTrack.position).magnitude;

    public int DebugID { get; set; }

    public void InitializeUponPrefabInstantiated(PoolOfMonoBehaviour<Building> pool) 
    {
        _pool = pool;

        // Create an array of arrays which excludes empty footprint parts.
        List<Vector2[]> footprint = new List<Vector2[]>();
        if (_footprintPart1.Length != 0)
            footprint.Add(new Vector2[_footprintPart1.Length]);
        if (_footprintPart2.Length != 0)
            footprint.Add(new Vector2[_footprintPart2.Length]);
        if (_footprintPart3.Length != 0)
            footprint.Add(new Vector2[_footprintPart3.Length]);
        if (_footprintPart4.Length != 0)
            footprint.Add(new Vector2[_footprintPart4.Length]);

        _footprint = footprint.ToArray();
    }

    public void InitializeUponProducedByPool() 
    {
        foreach (Transform t in _throwSources)
            _allThrowSources.Add(t);

        int index = 0;
        TryCalcNextPartOfFootprint(_footprintPart1);
        TryCalcNextPartOfFootprint(_footprintPart2);
        TryCalcNextPartOfFootprint(_footprintPart3);
        TryCalcNextPartOfFootprint(_footprintPart4);

        void TryCalcNextPartOfFootprint(Transform[] footprintPart)
        {
            if (footprintPart.Length != 0)
            {
                for (int i = 0; i < _footprintPart1.Length; i++)
                    _footprint[index][i] = _footprintPart1[i].position.To2D();
                index++;
            }
        }
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

    public static bool OverlappingFootprints(Building building1, Building building2)
    {
        // Check whether any of the polygons in the footprint of building1 overlap any of those of building2.
        for (int i = 0; i < building1._footprint.Length; i++)
        {
            for (int j = 0; j < building2._footprint.Length; j++)
            {
                if (VectorUtils.PolygonsOverlap2D(building1._footprint[i], building2._footprint[j]))
                    return true;
            }
        }
        return false;
    }
}
