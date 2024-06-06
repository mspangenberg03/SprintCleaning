using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevSettings : MonoBehaviour
{
    [field: SerializeField] public DevSettingsSO SO { get; private set; }

    private static DevSettings _instance;
    public static DevSettingsSO Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<DevSettings>();
                if (_instance == null)
                {
                    GameObject prefab = Resources.Load<GameObject>("Dev Settings");
                    Instantiate(prefab);
                    _instance = prefab.GetComponent<DevSettings>();
                }
            } 
            return _instance.SO;
        }
    }
}
