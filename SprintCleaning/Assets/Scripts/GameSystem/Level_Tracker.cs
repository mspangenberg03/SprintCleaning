using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Level_Tracker : MonoBehaviour
{
    [SerializeField]
    private int _unlockedLevels = 1;
    
    // Start is called before the first frame update
    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
    }

    public void UnlockLevel(){
        if(SceneManager.GetActiveScene().name == "Level 1"){
            _unlockedLevels = 2;
        }
        else if (SceneManager.GetActiveScene().name == "Level 2"){
            _unlockedLevels = 3;
        }
    }
    public int LevelsUnlocked(){
        return(_unlockedLevels);
    }
    

}
