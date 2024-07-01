using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IOnScoreChanges
{
    void OnScoreChanges(int newScore, int scoreChange);
}
