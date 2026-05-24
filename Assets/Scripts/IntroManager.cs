using UnityEngine;
using System.Collections;
using TMPro;

public class IntroManager : MonoBehaviour
{
    [Header("Camera")]
    public Camera mainCamera;
    public Camera faceCamera;

    [Header("Scripts to Disable")]
    public PlayerController_RB playerScript;
    public CameraFollow cameraFollowScript;

    [Header("UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    [Header("Text")]
    public float typingSpeed = 0.05f;
    public AudioClip typeSound;
    private AudioSource audioSource;

    [Header("Flight Settings")]
    public float transitionDuration = 2.0f;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        faceCamera.enabled = false;
        StartCoroutine(PlayIntroSequence());
    }

    IEnumerator PlayIntroSequence()
    {
        playerScript.enabled = false;
        cameraFollowScript.enabled = false;
        dialoguePanel.SetActive(false);

        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;

        yield return new WaitForSeconds(3f);

        yield return StartCoroutine(LerpCamera(startPos, startRot, faceCamera.transform.position, faceCamera.transform.rotation));

        dialoguePanel.SetActive(true);
        string message = "Hm... And where am I? It looks like an abandoned dump.";
        yield return StartCoroutine(TypeText(message));

        yield return new WaitForSeconds(2f);
        dialoguePanel.SetActive(false);

        yield return StartCoroutine(LerpCamera(mainCamera.transform.position, mainCamera.transform.rotation, startPos, startRot));

        
        playerScript.enabled = true;
        cameraFollowScript.enabled = true;
    }

    IEnumerator TypeText(string textToType)
    {
        dialogueText.text = "";

        foreach (char letter in textToType.ToCharArray())
        {
            dialogueText.text += letter;

            if (audioSource && typeSound)
            {
                
                audioSource.PlayOneShot(typeSound);
            }

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