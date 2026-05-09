using UnityEngine;

public class ButtonSpriteSwitch : MonoBehaviour
{
    [SerializeField, Tooltip("Default button object")]
    private GameObject defaultButton;

    [SerializeField, Tooltip("Active button object")]
    private GameObject activeButton;

    private void Start()
    {
        if (defaultButton != null) defaultButton.SetActive(true);
        if (activeButton != null) activeButton.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (defaultButton != null) defaultButton.SetActive(false);
        if (activeButton != null) activeButton.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (defaultButton != null) defaultButton.SetActive(true);
        if (activeButton != null) activeButton.SetActive(false);
    }
}