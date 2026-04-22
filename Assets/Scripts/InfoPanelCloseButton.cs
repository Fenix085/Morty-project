using UnityEngine;
using UnityEngine.UI;

public class InfoPanelCloseButton : MonoBehaviour
{
    [SerializeField] private GameObject targetPanel;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (targetPanel == null && transform.parent != null)
            targetPanel = transform.parent.gameObject;

        if (button != null)
            button.onClick.AddListener(ClosePanel);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(ClosePanel);
    }

    public void ClosePanel()
    {
        if (targetPanel != null)
            targetPanel.SetActive(false);
    }
}
