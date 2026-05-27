using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI Elements")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI textDisplay;
    public Image characterImage;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip typingSound;

    [Header("Settings")]
    [TextArea(3, 10)]
    public string[] lines; 
    public float typingSpeed = 0.04f; 

    private int index;
    public bool isDialogueActive = false;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        
        StartDialogue();
    }

    public void StartDialogue()
    {
        isDialogueActive = true;
        dialoguePanel.SetActive(true);
        index = 0;
        StartCoroutine(TypeLine());
    }

    void Update()
    {
        
        if (isDialogueActive && Input.GetMouseButtonDown(0))
        {
            
            if (textDisplay.text == lines[index])
            {
                NextLine();
            }
            else
            {
                
                StopAllCoroutines();
                textDisplay.text = lines[index];
            }
        }
    }

    IEnumerator TypeLine()
    {
        textDisplay.text = string.Empty;
        foreach (char c in lines[index].ToCharArray())
        {
            textDisplay.text += c;

            if (audioSource != null && typingSound != null && c != ' ')
            {
                
                audioSource.PlayOneShot(typingSound);
            }
            

            yield return new WaitForSeconds(typingSpeed);
        }
    }

    void NextLine()
    {
        if (index < lines.Length - 1)
        {
            index++;
            StartCoroutine(TypeLine());
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        isDialogueActive = false;
        dialoguePanel.SetActive(false);
        Debug.Log("Dialogue finisged");
    }
}