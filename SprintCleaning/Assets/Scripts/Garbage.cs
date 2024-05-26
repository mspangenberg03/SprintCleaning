using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Garbage : MonoBehaviour
{
    [SerializeField,Tooltip("The type of tool that is required to collect the garbage")]
    private GameObject _toolRequired;

    [SerializeField,Tooltip("How much dirtiness does this garbage add")]
    private int _dirtiness = 10;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //TODO: Check if the player has the right tools
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            other.transform.parent.gameObject.GetComponent<DirtinessManager>().AddDirtiness(_dirtiness);
        }
    }
}
