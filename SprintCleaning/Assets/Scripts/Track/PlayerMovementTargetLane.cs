using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementTargetLane
{
    private bool _discreteMovement;

    // These are only used for discrete movement, which needs input buffering to not miss key presses in frames without fixed update.
    private bool _polledInputsThisFrame;
    private int _leftKeyPresses;
    private int _rightKeyPresses;

    private bool LeftKeyDown => Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow);
    private bool RightKeyDown => Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow);
    private bool LeftKey => Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
    private bool RightKey => Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);

    public float? TargetLane { get; private set; } 


    public PlayerMovementTargetLane(bool discreteMovement)
    {
        _discreteMovement = discreteMovement;
    }

    public void Reset()
    {
        TargetLane = 0;
        _polledInputsThisFrame = false;
        _leftKeyPresses = 0;
        _rightKeyPresses = 0;
    }

    public void OnFixedUpdate()
    {
        if (!_discreteMovement)
            SetTargetLaneForContinuousMovement();
        else
            ChangeTargetLaneForDiscreteMovement();
    }

    private void SetTargetLaneForContinuousMovement()
    {
        if (RightKey == LeftKey)
            TargetLane = null;
        else if (RightKey)
            TargetLane = 1;
        else if (LeftKey)
            TargetLane = -1;
    }

    private void ChangeTargetLaneForDiscreteMovement()
    {
        CheckPollInput();
        TargetLane = Mathf.Clamp(TargetLane.Value + _rightKeyPresses - _leftKeyPresses, -1f, 1f);
        _rightKeyPresses = 0;
        _leftKeyPresses = 0;
    }

    public void OnUpdate()
    {
        if (!_discreteMovement)
            return;

        CheckPollInput();

        _polledInputsThisFrame = false; // update happens after fixed update, so this is the end of the frame for this code
    }

    private void CheckPollInput()
    {
        if (_polledInputsThisFrame)
            return;
        _polledInputsThisFrame = true;

        if (RightKeyDown)
            _rightKeyPresses++;
        if (LeftKeyDown)
            _leftKeyPresses++;
    }
}
