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

    [Header("Return Animation Settings")]
    public Transform zoomInPoint;
    public Transform zoomOutPoint;
    public float transitionSpeed = 1.0f;

    // Static variable persists between scene loads
    private static string lastVisitedFacilityId = "";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        canvas = GetComponentInChildren<Canvas>();
        if (canvas != null) canvas.gameObject.SetActive(false);

        LoadCompletionState();
        ApplyReturnStateIfCompleted();

        
        string currentId = GetCompletedKey();
        if (lastVisitedFacilityId == currentId)
        {
            lastVisitedFacilityId = ""; 
            StartCoroutine(PlayReturnAnimation());
        }
    }

    public void ChangeScene()
    {
        SavePlayerStateForScene();
        IsCompleted = true;
        SaveCompletionState();

        
        lastVisitedFacilityId = GetCompletedKey();

        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.SwitchScene(puzzleScene);
        else
            SceneManager.LoadScene(puzzleScene);
    }

    IEnumerator PlayReturnAnimation()
    {
   
        var player = PlayerController_RB.Instance;

  
        var mainCam = Camera.main;

        var followScript = FindFirstObjectByType<CameraFollow>();


        if (player == null) { Debug.LogError("Facility: Player not found"); yield break; }
        if (mainCam == null) { Debug.LogError("Facility: MainCamera not found! Check the MainCamera tag."); yield break; }
        if (followScript == null) { Debug.LogError("Facility: CameraFollow script not found on the scene!"); yield break; }

        player.enabled = false;
        followScript.enabled = false;


        mainCam.transform.position = zoomInPoint.position;
        mainCam.transform.rotation = zoomInPoint.rotation;


        yield return new WaitForSeconds(1.0f);


        yield return StartCoroutine(LerpCamera(mainCam.transform, zoomInPoint.position, zoomInPoint.rotation, zoomOutPoint.position, zoomOutPoint.rotation, 2.5f));

        yield return new WaitForSeconds(1.5f);

  
        float elapsed = 0;
        Vector3 startPos = mainCam.transform.position;
        Quaternion startRot = mainCam.transform.rotation;

        while (elapsed < 1.0f)
        {
            elapsed += Time.deltaTime * transitionSpeed;

      
            Vector3 targetPos = followScript.transform.position;
            Quaternion targetRot = followScript.transform.rotation;

            mainCam.transform.position = Vector3.Lerp(startPos, targetPos, elapsed);
            mainCam.transform.rotation = Quaternion.Slerp(startRot, targetRot, elapsed);

            yield return null;
        }

        
        player.enabled = true;
        followScript.enabled = true;
    }

    IEnumerator LerpCamera(Transform cam, Vector3 fromPos, Quaternion fromRot, Vector3 toPos, Quaternion toRot, float duration)
    {
        float time = 0;
        while (time < 1.0f)
        {
            time += Time.deltaTime / duration;
            cam.position = Vector3.Lerp(fromPos, toPos, time);
            cam.rotation = Quaternion.Slerp(fromRot, toRot, time);
            yield return null;
        }
        cam.position = toPos;
        cam.rotation = toRot;
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
