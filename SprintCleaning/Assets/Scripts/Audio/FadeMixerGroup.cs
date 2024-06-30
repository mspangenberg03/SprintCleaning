using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine;


/// This script is used to smoothly fade in and out mixer groups.
///
/// If you want to trigger a fade, use this line:
/// StartCoroutine(FadeMixerGroup.StartFade(AudioMixer audioMixer, String exposedParameter, float duration, float targetVolume));
///
/// You'll need to specify the Audio Mixer and the Exposed Parameter name for it to work.
/// This is static and can be called from anywhere, without needing an instance of the script in the scene.
/// You WILL need to reference the Audio Mixer and the Exposed Parameter Name when calling the script.
///
/// Read more about the tutorial this script came from at: https://johnleonardfrench.com/how-to-fade-audio-in-unity-i-tested-every-method-this-ones-the-best/#second_method


public static class FadeMixerGroup {
    public static IEnumerator StartFade(AudioMixer audioMixer, string exposedParam, float duration, float targetVolume)
    {
        targetVolume = Mathf.Clamp(targetVolume, 0.0001f, 1);

        float currentTime = 0;
        float currentVol;
        audioMixer.GetFloat(exposedParam, out currentVol);
        currentVol = Mathf.Pow(10, currentVol / 20);
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float newVol = Mathf.Lerp(currentVol, targetVolume, currentTime / duration);
            audioMixer.SetFloat(exposedParam, Mathf.Log10(newVol) * 20);
            yield return null;
        }
        yield break;
    }

    public static void SetVolume(AudioMixer audioMixer, string exposedParam, float newVolume)
    {
        newVolume = Mathf.Clamp(newVolume, 0.0001f, 1);
        audioMixer.SetFloat(exposedParam, Mathf.Log10(newVolume) * 20);
    }
}
