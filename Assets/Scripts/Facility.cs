using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Facility : MonoBehaviour
{
    private const string CompletedKeyPrefix = "FacilityCompleted_";
    private static readonly HashSet<string> CompletedKeys = new HashSet<string>();
    private Canvas canvas;
    public string puzzleScene;
    public bool IsCompleted = false;
    public string facilityId = "";
    public GameObject[] setInactiveOnReturn;
    public GameObject[] setActiveOnReturn;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        canvas = GetComponentInChildren<Canvas>();
        if (canvas != null)
        {
            canvas.gameObject.SetActive(false);
        }

        LoadCompletionState();
        ApplyReturnStateIfCompleted();
    }

    public void ChangeScene()
    {
        SavePlayerStateForScene();
        IsCompleted = true;
        SaveCompletionState();
        SceneManager.LoadScene(puzzleScene);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(Vector3.Distance(PlayerController_RB.Instance.transform.position, transform.position) < 6)
        {
            canvas.gameObject.SetActive(true);
        }
        else
        {
            canvas.gameObject.SetActive(false);
        }
    }

    private void LoadCompletionState()
    {
        IsCompleted = CompletedKeys.Contains(GetCompletedKey());
    }

    private void SaveCompletionState()
    {
        if (IsCompleted)
        {
            CompletedKeys.Add(GetCompletedKey());
        }
        else
        {
            CompletedKeys.Remove(GetCompletedKey());
        }
    }

    private void SavePlayerStateForScene()
    {
        var player = PlayerController_RB.Instance;
        if (player == null)
        {
            return;
        }

        var sceneName = SceneManager.GetActiveScene().name;
        SceneSessionState.SavePlayerState(sceneName, player.transform.position, player.transform.rotation);
    }

    private string GetCompletedKey()
    {
        var sceneName = SceneManager.GetActiveScene().name;
        var id = string.IsNullOrEmpty(facilityId) ? GetTransformPath(transform) : facilityId;
        return $"{CompletedKeyPrefix}{sceneName}_{id}";
    }

    private string GetTransformPath(Transform current)
    {
        if (current == null)
        {
            return "Unknown";
        }

        var path = current.name;
        var parent = current.parent;
        while (parent != null)
        {
            path = $"{parent.name}/{path}";
            parent = parent.parent;
        }

        return path;
    }

    private void ApplyReturnStateIfCompleted()
    {
        if (!IsCompleted)
        {
            return;
        }

        SetActiveForArray(setInactiveOnReturn, false);
        SetActiveForArray(setActiveOnReturn, true);
    }

    private void SetActiveForArray(GameObject[] targets, bool active)
    {
        if (targets == null)
        {
            return;
        }

        foreach (var target in targets)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }
    }
}
