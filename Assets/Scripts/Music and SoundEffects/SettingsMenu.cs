using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public AudioMixer mainMixer;
    public GameObject settingsPanel;

    [Header("Sliders")]
    public Slider musicSlider;
    public Slider sfxSlider;

    private void Start()
    {
        
        if (settingsPanel != null) settingsPanel.SetActive(false);

        
        if (mainMixer != null)
        {
            musicSlider.value = GetLevelFromMixer("MusicVol");
            sfxSlider.value = GetLevelFromMixer("SFXVol");
        }
    }

    
    private float GetLevelFromMixer(string parameterName)
    {
        float value;
        bool found = mainMixer.GetFloat(parameterName, out value);
        if (found)
        {
            
            return Mathf.Pow(10, value / 20);
        }
        return 1f; 
    }

    public void OpenMenu()
    {
        settingsPanel.SetActive(true);
    }

    public void CloseMenu()
    {
        settingsPanel.SetActive(false);
    }

    public void SetMusicVolume(float volume)
    {
        
        float v = Mathf.Max(volume, 0.0001f);
        mainMixer.SetFloat("MusicVol", Mathf.Log10(v) * 20);
    }

    public void SetSFXVolume(float volume)
    {
        float v = Mathf.Max(volume, 0.0001f);
        mainMixer.SetFloat("SFXVol", Mathf.Log10(v) * 20);
    }
}