using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    private AudioSource[] _audio;

    // Start is called before the first frame update
    void Start()
    {
        //Sets the volume of the audio sources to the audio volume set in the settings menu
        _audio = (AudioSource[])FindObjectsByType(typeof(AudioSource),FindObjectsSortMode.None);
        foreach(AudioSource source in _audio)
        {
            if(source != null)
            {
                source.volume = PlayerPrefs.GetFloat("AudioSettings", MainMenu.DEFAULT_VOLUME);
            }
        }
        //Sets the resolution to what was selected in the setting menu
        Screen.SetResolution(PlayerPrefs.GetInt("Width"), PlayerPrefs.GetInt("Height"),Screen.fullScreen);
        
    }
}
