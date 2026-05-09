using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelWinUI : MonoBehaviour
{
    public static LevelWinUI Instance;

    [Header("UI Elements")]
    public GameObject winPanel;
    public Image batteryDisplay;

    [Header("Sprites")]
    public List<Sprite> batteryFrames;

    [Header("Settings")]
    public float baseFrameDelay = 0.2f;
    public string nextSceneName;

    private void Awake()
    {
        Instance = this;
        if (winPanel != null) winPanel.SetActive(false);
    }

    [ContextMenu("Test Win")]
    public void ShowWinAnimation()
    {
        StopAllCoroutines();
        
        StartCoroutine(PlayWinSequence());
    }

    private IEnumerator PlayWinSequence()
    {
        winPanel.SetActive(true);

        
        float currentDelay = baseFrameDelay;
        for (int i = 0; i < batteryFrames.Count; i++)
        {
            batteryDisplay.sprite = batteryFrames[i];
            currentDelay *= 0.9f;
            yield return new WaitForSeconds(currentDelay);
        }

        
        batteryDisplay.transform.localScale = Vector3.one * 1.2f;
        yield return new WaitForSeconds(0.15f);
        batteryDisplay.transform.localScale = Vector3.one;

        
        
        yield return new WaitForSeconds(5f);

        
        LoadNextLevel();
    }

    private void LoadNextLevel()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    
}