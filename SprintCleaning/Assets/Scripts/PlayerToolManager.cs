using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerToolManager : MonoBehaviour
{
    [Tooltip("How many tools the player can hold")]
    [SerializeField] private int _numberOfTools;
    [SerializeField] private ToolBar _toolBar;

    public List<ToolBase> _heldTools = new();

    public void ToolUsed(ToolType toolType)
    {
        int index = -1;
        for (int i = 0; i < _heldTools.Count; i++)
        {
            if (_heldTools[i]._type == toolType)
            {
                index = i; 
                break;
            }
        }
        if (index == -1)
            throw new System.Exception("No tool in _heldTools is toolType.");

        _heldTools[index]._toolUses++;
        if (_heldTools[index]._toolUses == _heldTools[index]._durablity)
        {
            _heldTools.RemoveAt(index);
        }

        _toolBar.UpdateDisplayedInfo(_heldTools);
    }

    public void TryAddTool(PickUpTool pickupTool)
    {
        for (int i = 0; i < _heldTools.Count; i++)
        {
            if (pickupTool.ToolInfo._type == _heldTools[i]._type)
            {
                _heldTools[i]._toolUses = 0;
                Destroy(pickupTool.gameObject);
                _toolBar.UpdateDisplayedInfo(_heldTools);
                return;
            }
        }

        _heldTools.Insert(0, pickupTool.ToolInfo);
        Destroy(pickupTool.gameObject);

        if (_heldTools.Count > _numberOfTools)
        {
            _heldTools.RemoveAt(_heldTools.Count - 1);
        }

        _toolBar.UpdateDisplayedInfo(_heldTools);
    }

    public bool HasTool(ToolType toolType)
    {
        foreach (ToolBase tool in _heldTools)
        {
            if (tool._type == toolType)
                return true;
        }
        return false;
    }

    //public void RemoveToolInRange(PickUpTool pickupTool)
    //{
    //_toolsInPickUpRange.Remove(pickupTool);
    //}

    //public void TryAddTool()
    //{
    //    for (int i = _toolsInPickUpRange.Count - 1; i >= 0; i--)
    //    {
    //        if (_toolsInPickUpRange[i] == null)
    //            _toolsInPickUpRange.RemoveAt(i); // remove destroyed tools
    //    }

    //    if(_toolsInPickUpRange.Count > 0)
    //    {
    //        _toolList.Add(_toolsInPickUpRange[0].ToolInfo);
    //        //_toolText.text += _toolsInPickUpRange[0].ToolInfo._type.ToString() +", ";
    //        Destroy(_toolsInPickUpRange[0].gameObject);
    //        _toolsInPickUpRange.RemoveAt(0);
    //        if (_toolList.Count > _numberOfTools)
    //        {
    //            _toolList.RemoveAt(0);
    //        }
    //        _toolBar.DrawSpritesOnToolAdd();
    //    }


    //}



    // no durability for now. Need a durability bar to playtest whether it's fun or not.
    // May or may not be more fun if refill durability upon colliding with an already-held tool. Test with and without that.

    //public void ToolUsed(ToolBase tool)
    //{

    //tool._toolUses++;
    //if(tool._toolUses >= tool._durablity) 
    //{
    //    _toolList.Remove(tool);
    //}
    //}




}
