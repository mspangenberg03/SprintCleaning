using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : new()
{
    private Stack<T> _pool = new();

    public void ReturnToPool(T obj)
    {
        _pool.Push(obj);
    }

    public T ProduceObject()
    {
        if (_pool.Count == 0)
            return new T();
        return _pool.Pop();
    }
}
