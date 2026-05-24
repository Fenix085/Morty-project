using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeGrowth : MonoBehaviour
{
    [Header("Animation Settings")]
    public float growthDuration = 1.5f;
    
    [Tooltip("Maximum random delay before growing, so multiple trees don't pop up on the exact same frame.")]
    public float randomDelayMax = 1.0f;

    [Tooltip("The curve of the growth. Default has a slight bouncy overshoot.")]
    public AnimationCurve growthCurve = new AnimationCurve(
        new Keyframe(0f, 0f, 0f, 2f), 
        new Keyframe(0.7f, 1.1f, 0f, 0f), 
        new Keyframe(1f, 1f, -1f, 0f)
    );

    [Header("Hierarchy Settings")]
    [Tooltip("If true, it will animate all child objects individually instead of the parent object itself.")]
    public bool animateChildren = true;
    
    [Tooltip("If true, starts the grow animation automatically when the object is enabled.")]
    public bool playOnEnable = true;

    private Dictionary<Transform, Vector3> originalScales = new Dictionary<Transform, Vector3>();
    private bool isInitialized = false;

    void Awake()
    {
        if (!isInitialized)
        {
            InitializeScales();
        }
    }

    private void InitializeScales()
    {
        if (animateChildren)
        {
            foreach (Transform child in transform)
            {
                originalScales[child] = child.localScale != Vector3.zero ? child.localScale : Vector3.one;
            }
        }
        else
        {
            originalScales[transform] = transform.localScale != Vector3.zero ? transform.localScale : Vector3.one;
        }
        isInitialized = true;
    }

    void OnEnable()
    {
        if (isInitialized && playOnEnable)
        {
            foreach (var kvp in originalScales)
            {
                StartCoroutine(GrowRoutine(kvp.Key, kvp.Value));
            }
        }
    }

    private IEnumerator GrowRoutine(Transform target, Vector3 targetScale)
    {
        // Instantly shrink to 0 so we can scale up
        target.localScale = Vector3.zero;

        // Apply a random delay specific to this individual tree
        float delay = Random.Range(0f, randomDelayMax);
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        float elapsedTime = 0f;

        while (elapsedTime < growthDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / growthDuration;
            
            // Evaluate the animation curve for our bounce effect
            float scaleMultiplier = growthCurve.Evaluate(t);
            target.localScale = targetScale * scaleMultiplier;
            
            yield return null;
        }

        // Snap to exactly the original scale at the end
        target.localScale = targetScale;
    }

    // Optional: Call this safely from other scripts if you ever need to trigger it manually
    public void TriggerGrowth()
    {
        if (!gameObject.activeInHierarchy) return;
        StopAllCoroutines();
        
        if (!isInitialized) InitializeScales();
        
        foreach (var kvp in originalScales)
        {
            StartCoroutine(GrowRoutine(kvp.Key, kvp.Value));
        }
    }

    public void TriggerShrink()
    {
        if (!gameObject.activeInHierarchy) return;
        StopAllCoroutines();
        
        if (!isInitialized) InitializeScales();
        
        foreach (var kvp in originalScales)
        {
            StartCoroutine(ShrinkRoutine(kvp.Key, kvp.Value));
        }
    }

    private IEnumerator ShrinkRoutine(Transform target, Vector3 targetScale)
    {
        // Apply a random delay specific to this individual tree
        float delay = Random.Range(0f, randomDelayMax);
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        float elapsedTime = 0f;

        while (elapsedTime < growthDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / growthDuration;
            
            // Just reverse the curve calculation conceptually, or lerp to 0 smoothly
            float scaleMultiplier = 1f - t; 
            target.localScale = targetScale * scaleMultiplier;
            
            yield return null;
        }

        target.localScale = Vector3.zero;
    }
}
