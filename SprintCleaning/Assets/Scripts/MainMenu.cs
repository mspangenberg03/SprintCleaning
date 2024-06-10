using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject _mainMenuPanel;
    [SerializeField]
    private GameObject _settingsMenuPanel;

    [SerializeField]
    private Slider _audioSlider;

    [SerializeField]
    private Toggle _fullscreenToggle;

    [SerializeField]
    private TextMeshProUGUI _resolutionDropdownText;

    private TMP_Dropdown _dropdown;

    private int index;
    private void Start()
    {
        _audioSlider.value = PlayerPrefs.GetFloat("AudioSettings");
        Camera.main.GetComponent<AudioSource>().volume = _audioSlider.value;
        _fullscreenToggle.isOn = Screen.fullScreen;
        _dropdown = _resolutionDropdownText.transform.parent.GetComponent<TMP_Dropdown>();
        index = PlayerPrefs.GetInt("Index",0);
        _dropdown.value = index;
        _resolutionDropdownText.text =_dropdown.options[index].text;
        
    }
    public void StartButton()
    {
        SceneManager.LoadScene("Main");
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
        string[] str= _resolutionDropdownText.text.Split("x");
        int width = int.Parse(str[0]);
        int height = int.Parse(str[1]);
        PlayerPrefs.SetInt("Width", width);
        PlayerPrefs.SetInt("Height", height);
        Screen.SetResolution(width, height,_fullscreenToggle.isOn);
        index = _dropdown.value;
        PlayerPrefs.SetInt("Index", index);
    }
}
