using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Event")]
public class GameEvent : ScriptableObject
{
    public List<GameEventListener> _listeners = new List<GameEventListener>();

    public void Raise()
    {
        foreach (GameEventListener listener in _listeners) { 
            listener.OnEventRaised();
        }
    }

    public void RegisterListener(GameEventListener listener)
    {
        if (!_listeners.Contains(listener))
            _listeners.Add(listener);
    }

    public void UnregisterListener(GameEventListener listener)
    {
        if(_listeners.Contains(listener))
            _listeners.Remove(listener);
    }
}
