using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeMovement : MonoBehaviour {

    // inspector variables
    [SerializeField, Tooltip("Left Eye gameObject")]
    private GameObject leftEye;
    [SerializeField, Tooltip("Right Eye gameObject")]
    private GameObject rightEye;
    [SerializeField, Tooltip("Blink rate of eyes per minute")]
    private float blinkRate = 20;
    [SerializeField, Tooltip("Speed at which eyes close/open")]
    private float blinkSpeed = 5;
    [SerializeField, Tooltip("Randomises timing between blinks while roughly maintaining blink rate")]
    private bool randomiseRate = true;
    [SerializeField, Tooltip("Displays countdown between blinks")]
    public float blinkTimer = 60;
    
    // privates
    private Vector3 _leftDefaultScale = Vector3.zero;
    private Vector3 _rightDefaultScale = Vector3.zero;

    // Use this for initialization
    private void Start () {
        // get eyes initial scale
        GetEyeScale();
        // set blink cycle rate
        SetBlinkTimer();
    }
	
	// Update is called once per frame
	private void Update () {

        blinkTimer -= Time.deltaTime;

		if (blinkTimer <= 0f)
        {
            StartCoroutine("Blink");
            SetBlinkTimer();
        }
	}

    /// <summary>
    /// Blink coroutine, closes and opens the eye
    /// </summary>
    /// <returns></returns>
    private IEnumerator Blink()
    {
        yield return StartCoroutine("CloseEyes");
        yield return StartCoroutine("OpenEyes");
    }

    /// <summary>
    /// Close eye coroutine
    /// - reduces eye gameObject Z scale to zero over time
    /// </summary>
    /// <returns></returns>
    private IEnumerator CloseEyes()
    {
        bool eyeOpen = true;
        Vector3 eyeScale = leftEye.transform.localScale;

        while(eyeOpen)
        {
            // reduce eye scale
            eyeScale.z -= blinkSpeed * Time.deltaTime;
            // check if eye is closed
            if (eyeScale.z <= 0f)
            {
                // clamp eye scale
                eyeScale.z = 0; 
                // set loop flag to stop loop
                eyeOpen = false;
            }
            // set eyes to new scale
            leftEye.transform.localScale = eyeScale;
            rightEye.transform.localScale = eyeScale;

            yield return null;
        }
    }

    /// <summary>
    /// Open eye coroutine
    ///  -  Increases eye gameObject Z scale to original size over time
    /// </summary>
    /// <returns></returns>
    private IEnumerator OpenEyes()
    {
        bool eyeOpen = false;
        Vector3 eyeScale = leftEye.transform.localScale;
        Vector3 openScale = _leftDefaultScale.z > _rightDefaultScale.z ? _leftDefaultScale : _rightDefaultScale;

        while (!eyeOpen)
        {
            // increase eye scale
            eyeScale.z += blinkSpeed * Time.deltaTime;
            // check if eye is open
            if (eyeScale.z >= openScale.z)
            {
                // clamp eye scale
                eyeScale.z = openScale.z;
                // set loop flag to stop loop
                eyeOpen = true;
            }
            // set eyes to new scale
            leftEye.transform.localScale = eyeScale;
            rightEye.transform.localScale = eyeScale;

            yield return null;
        }
    }

    /// <summary>
    /// Gets the localScale of provided eye gameObjects
    /// - Logs error if gameObjects are null
    /// </summary>
    private void GetEyeScale()
    {
        // check if left eye exists and get scale
        if (leftEye != null)
        {
            _leftDefaultScale = leftEye.transform.localScale;
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogError("Left eye missing from EyeMovement script");
#endif
        }
        // check if right eye exists and get scale
        if (rightEye != null)
        {
            _rightDefaultScale = rightEye.transform.localScale;
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogError("Right eye missing from EyeMovement script");
#endif
        }
    }

    /// <summary>
    /// Set blink timer for next blink
    /// - adds random value if requested
    /// </summary>
    private void SetBlinkTimer()
    {
        blinkTimer = 60 / blinkRate;
        if (randomiseRate)
        {
            blinkTimer += (Random.value * blinkRate);
        }
    }
}
