using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevHelperRef : MonoBehaviour
{
    [field: SerializeField] public DevHelper SO { get; private set; }

    private void OnDestroy()
    {
        SO.OnDestroyRef();
    }
}
