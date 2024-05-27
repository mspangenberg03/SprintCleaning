using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Garbage : MonoBehaviour
{
    [Tooltip("The type of tool needed to collect this piece of garbage")]
    public ToolType _type;

    [SerializeField,Tooltip("How much dirtiness does this garbage add")]
    private int _dirtiness = 10;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GameObject player = other.transform.parent.gameObject;
            foreach (ToolBase tool in player.GetComponent<PlayerToolManager>().GetToolList())
            {
                if(tool._type == _type)
                {
                    player.GetComponent<PlayerToolManager>().ToolUsed(tool);
                    return;
                }
            }
            player.GetComponent<DirtinessManager>().AddDirtiness(_dirtiness);

        }
    }
}
