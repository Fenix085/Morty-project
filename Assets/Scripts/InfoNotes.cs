using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InfoNotes : MonoBehaviour
{
    [SerializeField, TextArea(3, 8)] private string noteText = "Write info text here.";
    [SerializeField] private string infoPanelName = "InfoPanel";
    [SerializeField] private bool hidePanelOnStart = true;
    [SerializeField] private float interactionDistance = 0f;

    private GameObject infoPanel;
    private TMP_Text plaqueText;
    private TMP_Text infoPanelText;

    private void Start()
    {
        ResolveInfoPanel();
        ApplyNoteTextToPlaque();

        if (hidePanelOnStart && infoPanel != null)
            infoPanel.SetActive(false);
    }

    private void OnValidate()
    {
        ApplyNoteTextToPlaque();
    }

    private void OnMouseDown()
    {
        ResolveInfoPanel();
        ApplyNoteTextToPlaque();

        if (infoPanel == null)
            return;

        if (interactionDistance > 0f && !IsPlayerCloseEnough())
            return;

        ApplyNoteTextToInfoPanel();
        infoPanel.SetActive(true);
    }

    private void ResolveInfoPanel()
    {
        if (infoPanel != null || string.IsNullOrWhiteSpace(infoPanelName))
            return;

        Transform[] sceneTransforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Transform sceneTransform in sceneTransforms)
        {
            if (sceneTransform.name == infoPanelName)
            {
                infoPanel = sceneTransform.gameObject;
                return;
            }
        }
    }

    private void ApplyNoteTextToPlaque()
    {
        ResolvePlaqueText();

        if (plaqueText != null)
            plaqueText.text = noteText;
    }

    private void ResolvePlaqueText()
    {
        if (plaqueText != null)
            return;

        TMP_Text[] textComponents = GetComponentsInChildren<TMP_Text>(true);
        if (textComponents.Length > 0)
            plaqueText = textComponents[0];
    }

    private void ApplyNoteTextToInfoPanel()
    {
        ResolveInfoPanelText();

        if (infoPanelText != null)
            infoPanelText.text = noteText;
    }

    private void ResolveInfoPanelText()
    {
        if (infoPanelText != null || infoPanel == null)
            return;

        TMP_Text[] textComponents = infoPanel.GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text textComponent in textComponents)
        {
            if (textComponent.GetComponentInParent<Button>() == null)
            {
                infoPanelText = textComponent;
                return;
            }
        }
    }

    private bool IsPlayerCloseEnough()
    {
        if (PlayerController_RB.Instance == null)
            return true;

        float distance = Vector3.Distance(PlayerController_RB.Instance.transform.position, transform.position);
        return distance <= interactionDistance;
    }
}
