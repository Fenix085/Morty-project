using UnityEngine;

public class ButtonSpriteSwitch : MonoBehaviour
{
    [SerializeField, Tooltip("Default button object")]
    private GameObject defaultButton;

    [SerializeField, Tooltip("Tags that can trigger the button (e.g. Player, Platform)")]
    private string[] triggerTags = { "Player" };

    private int _overlapCount = 0;

    private void Start()
    {
        if (defaultButton != null) defaultButton.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsValidTag(other)) return;
        _overlapCount++;
        if (defaultButton != null) defaultButton.SetActive(false);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsValidTag(other)) return;
        _overlapCount--;
        if (_overlapCount <= 0)
        {
            _overlapCount = 0;
            if (defaultButton != null) defaultButton.SetActive(true);
        }
    }

    private bool IsValidTag(Collider other)
    {
        foreach (var tag in triggerTags)
            if (other.CompareTag(tag)) return true;
        return false;
    }
}