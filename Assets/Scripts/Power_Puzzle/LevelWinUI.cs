using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelWinUI : MonoBehaviour
{
    public static LevelWinUI Instance;

    [Header("UI Elements")]
    
    public GameObject winPanel;

    [Header("Settings")]
    public string nextSceneName;

    private void Awake()
    {
        Instance = this;
        
        if (winPanel != null) winPanel.SetActive(false);
    }

    
    public void ShowWin()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
    }

    
    public void OnDoneButtonClick()
    {
        if (nextSceneName == "FakeGravity")
        {
            SceneSessionState.MarkCurrentFacilityCompleted();
        }

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextIndex < SceneManager.sceneCountInBuildSettings)
                SceneManager.LoadScene(nextIndex);
            
        }
    }
}