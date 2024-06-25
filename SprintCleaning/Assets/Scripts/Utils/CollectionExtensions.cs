using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CollectionExtensions
{
    public static T TakeRandomElement<T>(this List<T> list) 
    {
        int index = Random.Range(0, list.Count);
        T result = list[index];
        list.RemoveAt(index);
        return result;
    }

    public static T RandomElement<T>(this List<T> list)
    {
        return list[Random.Range(0, list.Count)];
    }

    public static T RandomElement<T>(this T[] array)
    {
        return array[Random.Range(0, array.Length)];
    }
}