using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackPiece : MonoBehaviour
{
    [field: SerializeField] public Transform Start { get; private set; } // Used for positioning this track piece when creating it
    [field: SerializeField] public Transform End { get; private set; } // Used for the player's target
}
