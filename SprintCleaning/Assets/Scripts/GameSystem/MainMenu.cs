using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MainMenu : MonoBehaviour
{
    public const float DEFAULT_VOLUME = .5f;

    [SerializeField]
    private GameObject _levelTracker;
    [SerializeField]
    private GameObject _mainMenuPanel;
    [SerializeField]
    private GameObject _settingsMenuPanel;
    [SerializeField]
    private GameObject _levelSelectPanel;

    [SerializeField]
    private Slider _audioSlider;

    [SerializeField]
    private Toggle _fullscreenToggle;

    [SerializeField]
    private TextMeshProUGUI _resolutionDropdownText;

    private TMP_Dropdown _dropdown;

    private int index;

    

    [SerializeField]
    private Button[] _levelButtons;
    
    
    
    private Level_Tracker _levelCode;

    private void Awake()
    {
        PlayerPrefs.DeleteAll();
    }

    private void Start()
    {
        float volume = PlayerPrefs.GetFloat("AudioSettings", DEFAULT_VOLUME);
        _audioSlider.value = volume;
        Camera.main.GetComponent<AudioSource>().volume = volume;
        _fullscreenToggle.isOn = Screen.fullScreen;
        _dropdown = _resolutionDropdownText.transform.parent.GetComponent<TMP_Dropdown>();
        List<string> list = new List<string>();
        foreach (Resolution res in Screen.resolutions)
        {
            list.Add(res.ToString());
        }
        _dropdown.AddOptions(list);
        index = PlayerPrefs.GetInt("Index", 0);
        _dropdown.value = index;
        _resolutionDropdownText.text = _dropdown.options[index].text;
        //ChangeResolution();
        _levelCode = _levelTracker.GetComponent<Level_Tracker>();
        Debug.Log("Levels Unlocked " + _levelCode.LevelsUnlocked());
        for (int i = 0; i < _levelButtons.Length; i++){
            _levelButtons[i].interactable = false;
        }
        Debug.Log("Levels Unlocked " + _levelCode.LevelsUnlocked());
        for (int i = 0; i < _levelCode.LevelsUnlocked(); i++){
            Debug.Log("Levels Unlocked " + _levelCode.LevelsUnlocked());
            Debug.Log("Level " + (i + 1) + " interactable");
            _levelButtons[i].interactable = true;
        }



    }
    public void StartButton()
    {
        _mainMenuPanel.SetActive(false);
        _levelSelectPanel.SetActive(true);
        //SceneManager.LoadScene("Level 1");
    }
    public void StartLevel(int Level)
    {
        SceneManager.LoadScene("Level " + Level);
    }
    
    public void ExitGame()
    {
#if UNITY_STANDALONE_WIN
        Application.Quit();
#endif
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    public void Settings()
    {
        _mainMenuPanel.SetActive(false);
        _settingsMenuPanel.SetActive(true);
    }
    public void ReturnToMenu()
    {
        _mainMenuPanel.SetActive(true);
        _settingsMenuPanel.SetActive(false);
        _levelSelectPanel.SetActive(false);
    }

    public void ChangeAudio()
    {

        Camera.main.GetComponent<AudioSource>().volume = _audioSlider.value;
        PlayerPrefs.SetFloat("AudioSettings", _audioSlider.value);

    }
    public void SetFullscreen()
    {
        Screen.fullScreen = _fullscreenToggle.isOn;

    }
    public void ChangeResolution()
    {
        int width = int.Parse(_resolutionDropdownText.text.Split(" x ")[0]);
        int height = int.Parse(_resolutionDropdownText.text.ToString().Split(" x ")[1].Split(" @ ")[0]);
        PlayerPrefs.SetInt("Width", width);
        PlayerPrefs.SetInt("Height", height);
        Screen.SetResolution(width, height, _fullscreenToggle.isOn);
        index = _dropdown.value;
        PlayerPrefs.SetInt("Index", index);
    }
    public void OpenGarbageSellingMenu()
    {
        SceneManager.LoadScene("SellingMenu");
    }
}
