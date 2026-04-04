using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Tracks")]
    public List<AudioClip> musicTracks = new List<AudioClip>();

    [Header("Delay Settings")]
    public float minDelay = 5f;
    public float maxDelay = 10f;

    [Header("Fade Settings")]
    public float fadeInTime = 3f;
    public float fadeOutTime = 6f;
    public float maxVolume = 0.7f;

    private bool isPlayingRoutine = false;
    private int lastTrackIndex = -1;

    void Start()
    {
        StartMusicLoop();
    }

    public void StartMusicLoop()
    {
        if (!isPlayingRoutine)
        {
            StartCoroutine(MusicLoop());
        }
    }
    private AudioClip GetRandomTrack()
    {
        if (musicTracks.Count == 0)
            return null;

        if (musicTracks.Count == 1)
            return musicTracks[0];

        int newIndex;

        do
        {
            newIndex = Random.Range(0, musicTracks.Count);
        }
        while (newIndex == lastTrackIndex);

        lastTrackIndex = newIndex;
        return musicTracks[newIndex];
    }
    private IEnumerator MusicLoop()
    {
        isPlayingRoutine = true;

        while (true)
        {
            float delay = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(delay);

            if (musicTracks.Count == 0)
                continue;

            AudioClip randomTrack = GetRandomTrack();

            audioSource.clip = randomTrack;
            audioSource.volume = 0f;
            audioSource.Play();

            yield return StartCoroutine(FadeIn());

            float playTime = randomTrack.length - fadeOutTime;
            if (playTime > 0)
                yield return new WaitForSeconds(playTime);

            yield return StartCoroutine(FadeOut());

            audioSource.Stop();
        }
    }
    private IEnumerator FadeIn()
    {
        float time = 0f;

        while (time < fadeInTime)
        {
            time += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, maxVolume, time / fadeInTime);
            yield return null;
        }

        audioSource.volume = maxVolume;
    }
    private IEnumerator FadeOut()
    {
        float startVolume = audioSource.volume;
        float time = 0f;

        while (time < fadeOutTime)
        {
            time += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, time / fadeOutTime);
            yield return null;
        }

        audioSource.volume = 0f;
    }
}