using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Level_Tracker : MonoBehaviour
{
    [SerializeField]
    private int _unlockedLevels = 1;
    [SerializeField]
    public int _currentLevel;
    [SerializeField]
    private int _totalNumberOfLevels = 3;
    private static Level_Tracker _instance;

    public int _nextLevelThreshold;

    [SerializeField]
    public bool _isLevelCompletedOnScore = false;

    public static Level_Tracker Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<Level_Tracker>();
                if (_instance == null)
                {
                    GameObject instantiated = Instantiate(Resources.Load<GameObject>("levelTracker"));
                    _instance = instantiated.GetComponent<Level_Tracker>();
                    if (_instance == null)
                        throw new System.Exception();
                }
            }
            return _instance;
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void UnlockLevel(){
        if(SceneManager.GetActiveScene().name == "Level 1" && LevelsUnlocked() < 2){
            _unlockedLevels = 2;
        }
        else if (SceneManager.GetActiveScene().name == "Level 2" && LevelsUnlocked() < 3)
        {
            _unlockedLevels = 3;
        }
    }
    public int LevelsUnlocked(){
        return(_unlockedLevels);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (SceneManager.GetActiveScene().buildIndex <= _totalNumberOfLevels)
            _currentLevel = SceneManager.GetActiveScene().buildIndex;
    }


}
