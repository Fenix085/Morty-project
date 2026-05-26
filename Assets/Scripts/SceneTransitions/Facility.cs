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

    [Header("Grass Growth")]
    public Transform grassObject;
    private Vector3 originalGrassScale = Vector3.one;
    [Header("Return Animation Settings")]
    public Transform zoomInPoint;
    public Transform zoomOutPoint;
    public float transitionSpeed = 1.0f;

    // Static variable persists between scene loads
    private static string lastVisitedFacilityId = "";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (grassObject != null)
        {
            originalGrassScale = grassObject.localScale;
            // Fallback just in case it was accidentally left at 0,0,0 in the editor
            if (originalGrassScale == Vector3.zero) 
                originalGrassScale = Vector3.one;
        }

        canvas = GetComponentInChildren<Canvas>();
        if (canvas != null) canvas.gameObject.SetActive(false);

        LoadCompletionState();
        if (SceneSessionState.JustCompletedFacilityId == GetEffectiveFacilityId())
        {
            if (groundRenderer != null)
                groundRenderer.material = barrenMaterial;

            if (grassObject != null)
            {
                grassObject.gameObject.SetActive(false);
                grassObject.localScale = Vector3.zero;
            }

            IsCompleted = true;
            SaveCompletionState();

            SceneSessionState.JustCompletedFacilityId = "";
            StartCoroutine(AnimateGroundTransition());
        }
        else
        {
            if (groundRenderer != null)
            {
                if (IsCompleted)
                    groundRenderer.material = greenMaterial;
                else
                    groundRenderer.material = barrenMaterial;
            }

            if (IsCompleted)
            {
                ApplyReturnStateIfCompleted();
                if (grassObject != null)
                {
                    grassObject.gameObject.SetActive(true);
                    grassObject.localScale = originalGrassScale;
                }
            }
            else
            {
                if (grassObject != null)
                {
                    grassObject.gameObject.SetActive(false);
                    grassObject.localScale = Vector3.zero;
                }
            }
        }

        // Check if returning from puzzle and trigger return animation
        string currentId = GetCompletedKey();
        if (lastVisitedFacilityId == currentId && zoomInPoint != null && zoomOutPoint != null)
        {
            lastVisitedFacilityId = ""; 
            StartCoroutine(PlayReturnAnimation());
        }
    }

    private IEnumerator AnimateGroundTransition()
    {
        // Trigger Shrink animation on dead trees if they have TreeGrowth script
        if (setInactiveOnReturn != null)
        {
            foreach (var target in setInactiveOnReturn)
            {
                if (target != null)
                {
                    TreeGrowth treeGrowth = target.GetComponent<TreeGrowth>();
                    if (treeGrowth != null)
                    {
                        treeGrowth.TriggerShrink();
                    }

                    DebrisFlyAnimation debrisFly = target.GetComponent<DebrisFlyAnimation>();
                    if (debrisFly != null)
                    {
                        debrisFly.FlyTo(transform.position);
                    }
                }
            }
        }

        Vector3 startScale = originalGrassScale * 0.9f;

        if (grassObject != null)
        {
            grassObject.gameObject.SetActive(true);
            grassObject.localScale = startScale;
        }

        if (groundRenderer != null && barrenMaterial != null && greenMaterial != null && transitionDuration > 0f)
        {
            Material tempMaterial = new Material(barrenMaterial);
            groundRenderer.material = tempMaterial;

            // Safely get colors to prevent URP exceptions
            Color startColor = Color.white;
            if (barrenMaterial.HasProperty("_BaseColor")) startColor = barrenMaterial.GetColor("_BaseColor");
            else if (barrenMaterial.HasProperty("_Color")) startColor = barrenMaterial.color;

            Color endColor = Color.white;
            if (greenMaterial.HasProperty("_BaseColor")) endColor = greenMaterial.GetColor("_BaseColor");
            else if (greenMaterial.HasProperty("_Color")) endColor = greenMaterial.color;

            float elapsedTime = 0f;

            while (elapsedTime < transitionDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / transitionDuration;

                // Explicitly interpolate colors without Material.Lerp which causes rendering glitches
                Color lerpedColor = Color.Lerp(startColor, endColor, t);

                if (tempMaterial.HasProperty("_BaseColor"))
                    tempMaterial.SetColor("_BaseColor", lerpedColor);
                else if (tempMaterial.HasProperty("_Color"))
                    tempMaterial.color = lerpedColor;

                if (grassObject != null)
                {
                    grassObject.localScale = Vector3.Lerp(startScale, originalGrassScale, t);
                }

                yield return null;
            }

            // Snap to the actual green material at the end
            groundRenderer.material = greenMaterial;
        }
        else
        {
            // Fallback if GroundRenderer or material is missing but we still want grass to grow
            float elapsedTime = 0f;
            while (elapsedTime < transitionDuration && transitionDuration > 0f)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / transitionDuration;
                if (grassObject != null)
                {
                    grassObject.localScale = Vector3.Lerp(startScale, originalGrassScale, t);
                }
                yield return null;
            }

            if (groundRenderer != null && greenMaterial != null)
            {
                groundRenderer.material = greenMaterial;
            }
        }

        if (grassObject != null)
        {
            grassObject.localScale = originalGrassScale;
        }

        ApplyReturnStateIfCompleted();
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

    public void ChangeScene()
    {
        SavePlayerStateForScene();
        SceneSessionState.CurrentFacilityId = GetEffectiveFacilityId();
        // IsCompleted = true;
        SaveCompletionState();

        
        lastVisitedFacilityId = GetCompletedKey();

        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.SwitchScene(puzzleScene);
        else
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

    public string GetEffectiveFacilityId()
    {
        return string.IsNullOrEmpty(facilityId) ? GetTransformPath(transform) : facilityId;
    }

    private string GetCompletedKey()
    {
        var sceneName = SceneManager.GetActiveScene().name;
        return $"{CompletedKeyPrefix}{sceneName}_{GetEffectiveFacilityId()}";
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