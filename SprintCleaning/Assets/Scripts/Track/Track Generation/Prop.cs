using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prop : MonoBehaviour
{
    [field: SerializeField] public Prop[] IncompatibleProps { get; private set; }
}
