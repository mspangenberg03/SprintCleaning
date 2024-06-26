using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Level_Tracker : MonoBehaviour
{

    private static bool _unlockLevel2 = false;
    

    private static bool _unlockLevel3 = false;
    // Start is called before the first frame update
    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
    }

    public void UnlockLevel(){
        if(SceneManager.GetActiveScene().name == "Level 1"){
            _unlockLevel2 = true;
        }
        else if (SceneManager.GetActiveScene().name == "Level 2"){
            _unlockLevel3 = true;
        }
    }
    public bool Level2Unlocked(){
        return(_unlockLevel2);
    }
    public bool Level3Unlocked(){
        return(_unlockLevel3);
    }

}
