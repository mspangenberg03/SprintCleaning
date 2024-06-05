using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract  class CustomUIComponent : MonoBehaviour
{

    public abstract void Configure();
    public abstract void Setup();


    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        Setup();
        Configure();
    }

    private void OnValidate()
    {
        Init();
    }
}
