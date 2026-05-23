using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [SerializeField, Tooltip("Next scene name")]
    private string nextLevel;

    [SerializeField, Tooltip("Delay before loading next scene")]
    private float delay = 1f;

    private LeverSwitch[] _levers;

    private void Start()
    {
        _levers = FindObjectsByType<LeverSwitch>(FindObjectsSortMode.None);

        if (_levers.Length == 0)
            Debug.LogWarning("LevelManager: No LeverSwitch found in scene!");
    }

    private void Update()
    {
        if (AllLeversActivated())
            StartCoroutine(LoadNextLevel());
    }

    private bool AllLeversActivated()
    {
        foreach (var lever in _levers)
        {
            // рычаг ещё не активирован — он всё ещё активен на сцене
            if (lever != null && lever.gameObject.activeSelf)
                return false;
        }
        return _levers.Length > 0;
    }

    private IEnumerator LoadNextLevel()
    {
        // отключаем Update чтобы корутина не запускалась повторно
        enabled = false;
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(nextLevel);
    }
}