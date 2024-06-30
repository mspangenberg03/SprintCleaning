using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerModelAnimatorRef : MonoBehaviour
{
    [field: SerializeField] public Animator Animator { get; private set; }

    private static PlayerModelAnimatorRef _instance;
    public static PlayerModelAnimatorRef Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<PlayerModelAnimatorRef>();
            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
    }
}
