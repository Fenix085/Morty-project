using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "SampleScene";

    public void PlayGame()
    {
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
}
