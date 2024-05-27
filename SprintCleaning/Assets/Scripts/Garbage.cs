using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Garbage : MonoBehaviour
{

    [SerializeField,Tooltip("How much dirtiness does this garbage add")]
    private int _dirtiness = 10;
    //TODO: Check if the player has the right tools
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            other.transform.parent.gameObject.GetComponent<DirtinessManager>().AddDirtiness(_dirtiness);
        }
    }
}
