using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class ToolBase : MonoBehaviour
{

    public ToolType _type;
    private bool _inPickupRange;
    // Start is called before the first frame update
    private void OnTriggerEnter(Collider other)
    {
        
    }

}
public enum ToolType
{
    Broom,
    GarbageBag,
    Mop,
    Sponge
}
