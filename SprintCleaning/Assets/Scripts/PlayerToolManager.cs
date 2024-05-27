using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerToolManager : MonoBehaviour
{
    
    private List<GameObject> _toolList; //The tools that the player currently is carrying
    [SerializeField,Tooltip("How many tools the player can hold")]
    private int _numberOfTools;
    // Start is called before the first frame update
    void Start()
    {
        _toolList = new List<GameObject>();
    }

    public void ToolAdded()
    {
        if (_toolList.Count > _numberOfTools)
        {
            _toolList.RemoveAt(0);
        }
    }

}
