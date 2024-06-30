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

    public AudioMixer GameAudioMixer => _gameAudioMixer;

    public double AudioStartTime { get; private set; }

    public static double CurrentAudioTime => ((double)Instance._musicSources[0].timeSamples) / Instance._musicSources[0].clip.frequency;

    private void Awake()
    {
        _ = DevHelper.Instance; // this is just to initialize it before anything uses Random, since this script has the earliest execution order.

        _instance = this;
        AudioStartTime = AudioSettings.dspTime + .5;
        for (int i = 0; i < _musicSources.Length; i++)
            _musicSources[i].PlayScheduled(AudioStartTime);
    }

    private void Start()
    {
        StartCoroutine(FadeMixerGroup.StartFade(_gameAudioMixer, "Streak1Volume", .5f, 1));
    }

    public void OnGameEnds()
    {
        float duration = .5f;
        StartCoroutine(FadeMixerGroup.StartFade(_gameAudioMixer, "Streak1Volume", duration, 0));
        StartCoroutine(FadeMixerGroup.StartFade(_gameAudioMixer, "Streak2Volume", duration, 0));
        StartCoroutine(FadeMixerGroup.StartFade(_gameAudioMixer, "Streak3Volume", duration, 0));
        StartCoroutine(FadeMixerGroup.StartFade(_gameAudioMixer, "Streak4Volume", duration, 0));
    }
}
