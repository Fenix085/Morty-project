using System.Collections;
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

    [Header("Ground Transition")]
    public Renderer groundRenderer;
    public float transitionDuration = 3f;
    public Material barrenMaterial;
    public Material greenMaterial;
    private Material groundMat;
    private bool hasAnimated = false;

    void Start()
    {
        canvas = GetComponentInChildren<Canvas>();
        if (canvas != null)
            canvas.gameObject.SetActive(false);

        LoadCompletionState();

        if (groundRenderer != null)
        {
            if (IsCompleted)
                groundRenderer.material = greenMaterial;
            else
                groundRenderer.material = barrenMaterial;
        }

        if (IsCompleted)
            ApplyReturnStateIfCompleted();
    }

    private IEnumerator AnimateGroundTransition()
    {
        Color startColor = barrenMaterial.color;
        Color endColor = greenMaterial.color;

        // Work on a copy so we don't modify the asset
        groundRenderer.material = new Material(barrenMaterial);

        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;
            groundRenderer.material.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }

        // Snap to the actual green material at the end
        groundRenderer.material = greenMaterial;
    }

    public void ChangeScene()
    {
        SavePlayerStateForScene();
        IsCompleted = true;
        SaveCompletionState();
        SceneManager.LoadScene(puzzleScene);
    }

    void FixedUpdate()
    {
        if (Vector3.Distance(PlayerController_RB.Instance.transform.position, transform.position) < 6)
            canvas.gameObject.SetActive(true);
        else
            canvas.gameObject.SetActive(false);
    }

    #if UNITY_EDITOR
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (PlayerController_RB.Instance == null) return;
            if (Vector3.Distance(PlayerController_RB.Instance.transform.position, transform.position) > 6f) return;

            // Reset
            if (groundRenderer != null)
                groundRenderer.material = barrenMaterial;
            SetActiveForArray(setInactiveOnReturn, true);
            SetActiveForArray(setActiveOnReturn, false);
            StopAllCoroutines();

            IsCompleted = true;
            SaveCompletionState();
            StartCoroutine(AnimateGroundTransition());
        }
    }
    #endif

    private void LoadCompletionState()
    {
        IsCompleted = CompletedKeys.Contains(GetCompletedKey());
    }

    private void SaveCompletionState()
    {
        if (IsCompleted)
            CompletedKeys.Add(GetCompletedKey());
        else
            CompletedKeys.Remove(GetCompletedKey());
    }

    private void SavePlayerStateForScene()
    {
        var player = PlayerController_RB.Instance;
        if (player == null) return;
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
        if (current == null) return "Unknown";
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
        if (!IsCompleted) return;
        SetActiveForArray(setInactiveOnReturn, false);
        SetActiveForArray(setActiveOnReturn, true);
    }

    private void SetActiveForArray(GameObject[] targets, bool active)
    {
        if (targets == null) return;
        foreach (var target in targets)
            if (target != null)
                target.SetActive(active);
    }
}