using UnityEngine;

public class ButtonSpriteSwitch : MonoBehaviour
{
    [SerializeField, Tooltip("Default button object")]
    private GameObject defaultButton;

    [SerializeField, Tooltip("Active button object")]
    private GameObject activeButton;

    [SerializeField, Tooltip("Sound on press")]
    private AudioClip pressSound;

    [SerializeField, Tooltip("Sound on release")]
    private AudioClip releaseSound;

    [SerializeField, Tooltip("Tags that can trigger the button (e.g. Player, Platform)")]
    private string[] triggerTags = { "Player" };

    private int _overlapCount = 0;
    private AudioSource _audioSource;

    private void Start()
    {
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;

        if (defaultButton != null) defaultButton.SetActive(true);
        if (activeButton != null) activeButton.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsValidTag(other)) return;
        _overlapCount++;
        if (pressSound != null) _audioSource.PlayOneShot(pressSound);
        if (defaultButton != null) defaultButton.SetActive(false);
        if (activeButton != null) activeButton.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsValidTag(other)) return;
        _overlapCount--;
        if (_overlapCount <= 0)
        {
            _overlapCount = 0;
            if (releaseSound != null) _audioSource.PlayOneShot(releaseSound);
            if (defaultButton != null) defaultButton.SetActive(true);
            if (activeButton != null) activeButton.SetActive(false);
        }
    }

    private bool IsValidTag(Collider other)
    {
        foreach (var tag in triggerTags)
            if (other.CompareTag(tag)) return true;
        return false;
    }
}