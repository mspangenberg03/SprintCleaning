using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PickUpTool : MonoBehaviour
{
    [field:SerializeField]
    public ToolBase ToolInfo { get; private set; }

    private int _overlappingColliders = 0; // This way, even if the player has multiple colliders, it'll behave as 1 collider gameplay-wise.
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player")) // should use collision layer matrix in physics settings instead of a tag
        {
            if (_overlappingColliders == 0)
            {
                other.transform.parent.GetComponent<PlayerToolManager>().TryAddTool(this);
            }
            _overlappingColliders++;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _overlappingColliders--;
        }
    }
}
