using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class IntroManager : MonoBehaviour
{
    [Header("Cameras")]
    public Camera mainCamera;
    public Camera faceCamera;

    [Header("Scripts to Disable")]
    public PlayerController_RB playerScript;
    public CameraFollow cameraFollowScript;

    [Header("UI Elements")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public Button skipButton;

    [Header("Text Settings")]
    public float typingSpeed = 0.05f;
    public AudioClip typeSound;
    private AudioSource audioSource;

    [Header("Transition Settings")]
    public float transitionDuration = 2.0f;

    private Coroutine introCoroutine;
    private Vector3 startPos;
    private Quaternion startRot;

    // Key to save the state
    private const string IntroWatchedKey = "IntroWatched";

    [Header("Debug")]
    public bool resetIntroOnStart = false; 
    public bool forceShowIntro = false;    

    void Start()
    {
        
        if (resetIntroOnStart)
        {
            PlayerPrefs.DeleteKey(IntroWatchedKey);
        }

        faceCamera.enabled = false;

        
        if (!forceShowIntro && PlayerPrefs.GetInt(IntroWatchedKey, 0) == 1)
        {
            startPos = mainCamera.transform.position;
            startRot = mainCamera.transform.rotation;
            EndIntro();
        }
        else
        {
            introCoroutine = StartCoroutine(PlayIntroSequence());
        }
    }

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (skipButton != null)
            skipButton.onClick.AddListener(SkipIntro);
    }

    

    IEnumerator PlayIntroSequence()
    {
        playerScript.enabled = false;
        cameraFollowScript.enabled = false;
        dialoguePanel.SetActive(false);

        if (skipButton != null)
            skipButton.gameObject.SetActive(true);

        startPos = mainCamera.transform.position;
        startRot = mainCamera.transform.rotation;

        yield return new WaitForSeconds(3f);

        yield return StartCoroutine(LerpCamera(startPos, startRot, faceCamera.transform.position, faceCamera.transform.rotation));

        dialoguePanel.SetActive(true);
        string message = "Hm... And where am I? It looks like an abandoned dump.";
        yield return StartCoroutine(TypeText(message));

        yield return new WaitForSeconds(2f);

        dialoguePanel.SetActive(false);

        yield return StartCoroutine(LerpCamera(mainCamera.transform.position, mainCamera.transform.rotation, startPos, startRot));

        // Save that the player has watched the intro
        MarkIntroAsWatched();
        EndIntro();
    }

    public void SkipIntro()
    {
        if (introCoroutine != null)
            StopCoroutine(introCoroutine);

        StopAllCoroutines();
        MarkIntroAsWatched(); // Save state even on skip
        EndIntro();
    }

    private void MarkIntroAsWatched()
    {
        PlayerPrefs.SetInt(IntroWatchedKey, 1);
        PlayerPrefs.Save();
    }

    private void EndIntro()
    {
        mainCamera.transform.position = startPos;
        mainCamera.transform.rotation = startRot;

        dialoguePanel.SetActive(false);

        if (skipButton != null)
            skipButton.gameObject.SetActive(false);

        playerScript.enabled = true;
        cameraFollowScript.enabled = true;
    }

    // Coroutines for TypeText and LerpCamera remain the same...
    IEnumerator TypeText(string textToType)
    {
        dialogueText.text = "";
        foreach (char letter in textToType.ToCharArray())
        {
            dialogueText.text += letter;
            if (audioSource && typeSound) audioSource.PlayOneShot(typeSound);
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    IEnumerator LerpCamera(Vector3 fromPos, Quaternion fromRot, Vector3 toPos, Quaternion toRot)
    {
        float time = 0;
        while (time < 1.0f)
        {
            time += Time.deltaTime / transitionDuration;
            mainCamera.transform.position = Vector3.Lerp(fromPos, toPos, time);
            mainCamera.transform.rotation = Quaternion.Slerp(fromRot, toRot, time);
            yield return null;
        }
        mainCamera.transform.position = toPos;
        mainCamera.transform.rotation = toRot;
    }
}