using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[DefaultExecutionOrder(-11)] // should be before anything accesses this in case the singleton's FindObjectOfTime being slow causes sync issues
public class GameplayMusic : MonoBehaviour
{
    [SerializeField] private AudioMixer _gameAudioMixer;
    public AudioSource[] _musicSources;

    private static GameplayMusic _instance;
    public static GameplayMusic Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<GameplayMusic>();
            return _instance;
        }
    }

    public double AudioStartTime { get; private set; }

    public static double CurrentAudioTime => ((double)Instance._musicSources[0].timeSamples) / Instance._musicSources[0].clip.frequency;

    private void Awake()
    {
        FadeMixerGroup.SetVolume(_gameAudioMixer, "Streak1Volume", 0);
        FadeMixerGroup.SetVolume(_gameAudioMixer, "Streak2Volume", 0);
        FadeMixerGroup.SetVolume(_gameAudioMixer, "Streak3Volume", 0);
        FadeMixerGroup.SetVolume(_gameAudioMixer, "Streak4Volume", 0);
        ScoreManager.Instance.CheckStreakMultiplier();

        _instance = this;
        //System.Threading.Thread.MemoryBarrier();
        AudioStartTime = AudioSettings.dspTime + .5;
        //System.Threading.Thread.MemoryBarrier();
        for (int i = 0; i < _musicSources.Length; i++)
            _musicSources[i].PlayScheduled(AudioStartTime);
    }
}
