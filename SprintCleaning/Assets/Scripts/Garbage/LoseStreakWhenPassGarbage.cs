using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoseStreakWhenPassGarbage : MonoBehaviour
{
    private bool _lost;
    private void OnDisable()
    {
        _lost = false; // when return to pool
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_lost)
            return;

        if (!other.gameObject.CompareTag("Player"))
            return;

        if (Game_Over.Instance.GameIsOver || Game_Over.Instance.LevelIsComplete)
            return;

        _lost = true;

        ScoreManager.Instance.OnPassGarbage();
    }

}
