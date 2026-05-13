using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffectsManager : MonoBehaviour
{
    public static SoundEffectsManager Instance;
    [Header("Audio Source")]
    public AudioSource sfxSource;

    [Header("Clips")]
    public List<AudioClip> soundEffects = new List<AudioClip>();

    [Header("Settings")]
    public float volume = 1f;
    public bool randomPitch = true;
    public float minPitch = 0.9f;
    public float maxPitch = 1.1f;

    public AudioClip vacuumSound;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    
    //Play sound effect by name
    public void PlayRandom()
    {
        if (soundEffects.Count == 0) return;

        AudioClip clip = soundEffects[Random.Range(0, soundEffects.Count)];
        Play(clip);
    }

    public AudioSource PlayLoopingSound(AudioClip clip, float volume)
    {
        if (clip == null) return null;

        
        GameObject sndObj = new GameObject("TempAudio_" + clip.name);
        AudioSource source = sndObj.AddComponent<AudioSource>();

        source.clip = clip;
        source.volume = volume;
        source.loop = true;
        source.Play();

        return source;
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