using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PickUpTool : MonoBehaviour
{
    [field:SerializeField]
    public ToolBase ToolInfo { get; private set; }

    private int _overlappingColliders = 0;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (_overlappingColliders == 0)
            {
                other.transform.parent.GetComponent<PlayerToolManager>()._toolsInPickUpRange.Add(this);
                
            }
            _overlappingColliders++;

        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _overlappingColliders--;
            if (_overlappingColliders == 0)
            {
                other.transform.parent.GetComponent<PlayerToolManager>()._toolsInPickUpRange.Remove(this);
            }

        }
    }
}
