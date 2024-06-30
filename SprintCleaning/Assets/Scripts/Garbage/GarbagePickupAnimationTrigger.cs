using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GarbagePickupAnimationTrigger : MonoBehaviour
{
    [SerializeField] private Garbage _garbage;

    private int _enterCount;

    public void Reset()
    {
        _enterCount = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _enterCount++;
            if (_enterCount != 1)
                return;
            Animator animator = PlayerModelAnimatorRef.Instance.Animator;
            if (!_garbage.Obstacle)
                animator.SetTrigger("PickUp");
        }
    }

    public void CheckTriggerAnimation(Collider other)
    {
        if (_enterCount != 0)
            return;
        Animator animator = PlayerModelAnimatorRef.Instance.Animator;
        if (!_garbage.Obstacle)
            animator.SetTrigger("PickUp");
    }

    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.gameObject.CompareTag("Player"))
    //        _enterCount--;
    //}
}
