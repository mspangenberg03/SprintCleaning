using System.Collections;
using System.Collections.Generic;
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

    private bool _exitingGame = false;

    
    private void Start()
    {
        _audioSlider.value = PlayerPrefs.GetFloat("AudioSettings");
        Camera.main.GetComponent<AudioSource>().volume = _audioSlider.value;
        
    }
    public void StartButton()
    {
        SceneManager.LoadScene("Main");
    }
    public void ExitGame()
    {
        _exitingGame=true;
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
        if (!_exitingGame)
        {
            Camera.main.GetComponent<AudioSource>().volume = _audioSlider.value;
            PlayerPrefs.SetFloat("AudioSettings", _audioSlider.value);
        }
    }
}
