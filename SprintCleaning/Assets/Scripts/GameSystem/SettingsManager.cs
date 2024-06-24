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
        _audio = (AudioSource[])FindObjectsByType(typeof(AudioSource), FindObjectsSortMode.None);
        foreach (AudioSource source in _audio)
        {
            if (source != null)
            {
                source.volume = PlayerPrefs.GetFloat("AudioSettings", MainMenu.DEFAULT_VOLUME);
            }
        }
        //Sets the resolution to what was selected in the setting menu
        int width = PlayerPrefs.GetInt("Width", -1);
        int height = PlayerPrefs.GetInt("Height", -1);
        if (width != -1 && height != -1)
        {
            Resolution current = Screen.currentResolution;
            if (current.width != width || current.height != height)
            {
                foreach (Resolution next in Screen.resolutions)
                {
                    if (next.width == width && next.height == height)
                    {
                        Screen.SetResolution(width, height, Screen.fullScreenMode, current.refreshRateRatio);
                        break;

                    }
                }
            }
        }
    }
}
