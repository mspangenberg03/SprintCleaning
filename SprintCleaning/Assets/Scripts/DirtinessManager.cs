using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DirtinessManager : MonoBehaviour
{
    private int _currentDirtiness = 0;
    [SerializeField,Tooltip("The amount of dirtiness")]
    private int _maxDirtiness;

    [SerializeField,Tooltip("The dirtiness bar of the player")]
    private GameObject _dirtinessBar;

    private float _percentOfDirtiness;
    //This function should be called from either the player script or the garbage script
    //Param: dirtiness - The amount of dirtiness that the piece of trash adds to the player
    public void AddDirtiness(int addedDirtiness)
    {
        _currentDirtiness += addedDirtiness;
        Debug.Log(_currentDirtiness);
        if(_currentDirtiness >= _maxDirtiness)
        {
            GameOver();
        }
        //else
        //{
            //_percentOfDirtiness = addedDirtiness / _maxDirtiness;
            //AdjustHealthBar();
        //}
    }
    
    private void GameOver()
    {
        Debug.Log("Game Over");
        SceneManager.LoadScene("MainMenu");
    }
    //TODO: Create health bar
    private void AdjustHealthBar()
    {
        Transform bar = _dirtinessBar.transform;
        _dirtinessBar.transform.localScale = new Vector3(bar.localScale.x - (bar.localScale.x * _percentOfDirtiness), bar.localScale.y, bar.localScale.z);
    }
}