using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Set the cinemachine brain update mode after the first frame or two.
// Just setting it in the inspector causes the camera to be at the wrong position on the 1st frame.

// Need to use FixedUpdate mode to avoid some really weird occaisional jitter in the player's position on the screen.

public class ChangeCameraUpdateModeAfterFirstUpdate : MonoBehaviour
{
    [SerializeField] private Cinemachine.CinemachineBrain _brain;

    private bool _updated;
    private void LateUpdate()
    {
        if (_updated)
        {
            _brain.m_UpdateMethod = Cinemachine.CinemachineBrain.UpdateMethod.FixedUpdate;
            _brain.m_BlendUpdateMethod = Cinemachine.CinemachineBrain.BrainUpdateMethod.FixedUpdate;
        }
        _updated = true;
    }
}
