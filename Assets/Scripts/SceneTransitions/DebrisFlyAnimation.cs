using System.Collections;
using UnityEngine;

public class DebrisFlyAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    public float flyDuration = 2f;
    public float arcHeight = 5f;
    public bool shrinkWhileFlying = true;

    [Header("Cluster Settings")]
    [Tooltip("If true, all children of this object will fly independently.")]
    public bool animateChildrenIndividually = true;
    [Tooltip("Maximum delay before a piece of debris starts flying.")]
    public float maxRandomDelay = 1.0f;
    [Tooltip("Slightly randomize the speed of each piece of debris.")]
    public bool randomizeSpeed = true;

    [Header("Planet / Targeting")]
    [Tooltip("If true, the arc and offset are calculated radially outward from a planet center.")]
    public bool isPlanet = true;
    public Vector3 planetCenter = Vector3.zero;
    [Tooltip("Height offset above the target facility point.")]
    public float targetOffsetHeight = 1f;

    public void FlyTo(Vector3 targetPosition)
    {
        // If there's already a coroutine running (e.g. called multiple times), stop it just in case
        StopAllCoroutines();
        
        Vector3 finalTargetPos = targetPosition;
        if (isPlanet)
        {
            finalTargetPos += (targetPosition - planetCenter).normalized * targetOffsetHeight;
        }
        else
        {
            finalTargetPos += new Vector3(0, targetOffsetHeight, 0);
        }

        if (animateChildrenIndividually && transform.childCount > 0)
        {
            StartCoroutine(FlyChildrenCoroutine(finalTargetPos));
        }
        else
        {
            StartCoroutine(FlyCoroutine(finalTargetPos));
        }
    }

    private IEnumerator FlyCoroutine(Vector3 targetPos)
    {
        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;
        
        float time = 0f;

        while (time < flyDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / flyDuration);

            // Calculate standard linear interpolation
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);

            // Formula for a simple parabolic arc:
            // equation: height * 4 * t * (1 - t) => peaks at t=0.5 with value 'height'
            float parabola = arcHeight * 4f * t * (1f - t);
            
            Vector3 upDir = isPlanet ? (currentPos - planetCenter).normalized : Vector3.up;
            currentPos += upDir * parabola;

            transform.position = currentPos;

            // Optional shrinking effect as it flies into the facility
            if (shrinkWhileFlying)
            {
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            }

            yield return null;
        }

        // Finalize its position just in case
        transform.position = targetPos;
        if (shrinkWhileFlying)
        {
            transform.localScale = Vector3.zero;
        }
        
        gameObject.SetActive(false);
    }

    private IEnumerator FlyChildrenCoroutine(Vector3 targetPos)
    {
        int count = transform.childCount;
        Transform[] children = new Transform[count];
        Vector3[] startPositions = new Vector3[count];
        Vector3[] startScales = new Vector3[count];
        float[] delays = new float[count];
        float[] durations = new float[count];

        float maxTotalTime = 0f;

        // Initialize random values for each child
        for (int i = 0; i < count; i++)
        {
            children[i] = transform.GetChild(i);
            startPositions[i] = children[i].position;
            startScales[i] = children[i].localScale;
            
            delays[i] = Random.Range(0f, maxRandomDelay);
            
            // Randomize duration between 80% and 120% of base duration
            durations[i] = randomizeSpeed ? flyDuration * Random.Range(0.8f, 1.2f) : flyDuration;
            
            float totalTime = delays[i] + durations[i];
            if (totalTime > maxTotalTime) maxTotalTime = totalTime;
        }

        float time = 0f;
        while (time < maxTotalTime)
        {
            time += Time.deltaTime;
            
            for (int i = 0; i < count; i++)
            {
                if (children[i] == null || !children[i].gameObject.activeSelf) continue;

                // Wait for this specific child's delay to pass
                if (time < delays[i]) continue;

                float t = Mathf.Clamp01((time - delays[i]) / durations[i]);

                Vector3 currentPos = Vector3.Lerp(startPositions[i], targetPos, t);
                float parabola = arcHeight * 4f * t * (1f - t);
                
                Vector3 upDir = isPlanet ? (currentPos - planetCenter).normalized : Vector3.up;
                currentPos += upDir * parabola;

                children[i].position = currentPos;

                if (shrinkWhileFlying)
                {
                    children[i].localScale = Vector3.Lerp(startScales[i], Vector3.zero, t);
                }

                if (t >= 1f)
                {
                    children[i].gameObject.SetActive(false);
                }
            }

            yield return null;
        }
    }
}

