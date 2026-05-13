using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "SampleScene";
    [SerializeField] private AudioClip clickClip;

    public void PlayGame()
    {
        PlayClickSound();
        if (string.IsNullOrWhiteSpace(gameSceneName))
            return;

        try
        {
            if (Application.CanStreamedLevelBeLoaded(gameSceneName))
                SceneManager.LoadScene(gameSceneName);
        }
        catch
        {
        }
    }

    public void CloseGame()
    {
        PlayClickSound();
        try
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        catch
        {
        }
    }


    private void PlayClickSound()
    {
        if (SoundEffectsManager.Instance != null && clickClip != null)
        {
            SoundEffectsManager.Instance.Play(clickClip);
        }
    }
}
