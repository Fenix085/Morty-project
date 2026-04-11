using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffectsManager : MonoBehaviour
{
    [Header("Audio Source")]
    public AudioSource sfxSource;

    [Header("Clips")]
    public List<AudioClip> soundEffects = new List<AudioClip>();

    [Header("Settings")]
    public float volume = 1f;
    public bool randomPitch = true;
    public float minPitch = 0.9f;
    public float maxPitch = 1.1f;

    //Play sound effect by name
    public void PlayRandom()
    {
        if (soundEffects.Count == 0) return;

        AudioClip clip = soundEffects[Random.Range(0, soundEffects.Count)];
        Play(clip);
    }

    //Play specific sound effect
    public void Play(AudioClip clip)
    {
        if (clip == null) return;

        if (randomPitch)
        {
            sfxSource.pitch = Random.Range(minPitch, maxPitch);
        }
        else
        {
            sfxSource.pitch = 1f;
        }

        sfxSource.PlayOneShot(clip, volume);
    }
}