using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerToolManager : MonoBehaviour
{
    
    private List<ToolBase> _toolList = new(); //The tools that the player currently is carrying
    [SerializeField,Tooltip("How many tools the player can hold")]
    private int _numberOfTools;
    public List<PickUpTool> _toolsInPickUpRange = new();
    // Start is called before the first frame update
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryAddTool();
        }
    }

    public void TryAddTool()
    {
 
        if(_toolsInPickUpRange.Count > 0)
        {
            _toolList.Add(_toolsInPickUpRange[0].ToolInfo);
            Destroy(_toolsInPickUpRange[0].gameObject);
            if (_toolList.Count > _numberOfTools)
            {
                _toolList.RemoveAt(0);
            }
        }
        

    }
    public void ToolUsed(ToolBase tool)
    {
        tool._toolUses++;
        if(tool._toolUses >= tool._durablity) 
        {
            _toolList.Remove(tool);
        }
    }
        
    
    public List<ToolBase> GetToolList()
    {
        return _toolList;
    }
    

}
