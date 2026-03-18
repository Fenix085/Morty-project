using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TrashCounterUI : MonoBehaviour
{
    public static TrashCounterUI Instance { get; private set; }

    [SerializeField] private TMP_Text counterText;
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private string counterFormat = "{0} / {1}";

    private readonly HashSet<int> collectedTrashIds = new();
    private int totalTrashCount;
    private int collectedTrashCount;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        try
        {
            if (Instance != null)
                return;

            if (CountTrashObjects() == 0)
                return;

            var bootstrapObject = new GameObject("TrashCounterUI");
            bootstrapObject.AddComponent<TrashCounterUI>();
        }
        catch
        {
        }
    }

    public static TrashCounterUI GetOrCreate()
    {
        try
        {
            if (Instance != null)
                return Instance;

            var existingInstance = FindAnyObjectByType<TrashCounterUI>();
            if (existingInstance != null)
                return existingInstance;

            var bootstrapObject = new GameObject("TrashCounterUI");
            return bootstrapObject.AddComponent<TrashCounterUI>();
        }
        catch
        {
            return null;
        }
    }

    private void Awake()
    {
        try
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;

            if (targetCanvas == null)
                targetCanvas = GetComponentInChildren<Canvas>(true);

            if (counterText == null)
                counterText = FindBestCounterText();

            if (counterText == null)
                counterText = CreateCounterText();

            totalTrashCount = CountTrashObjects();
            UpdateCounter();
        }
        catch
        {
            if (Instance == this)
                Instance = null;

            if (gameObject.name == "TrashCounterUI")
                Destroy(gameObject);
            else
                Destroy(this);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void RegisterCollectedTrash(GameObject trash)
    {
        try
        {
            if (trash == null)
                return;

            if (!collectedTrashIds.Add(trash.GetInstanceID()))
                return;

            collectedTrashCount = Mathf.Min(collectedTrashCount + 1, totalTrashCount);
            UpdateCounter();
        }
        catch
        {
        }
    }

    private TMP_Text FindBestCounterText()
    {
        try
        {
            TMP_Text fallback = null;
            var textComponents = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var textComponent in textComponents)
            {
                if (textComponent == null)
                    continue;

                string lowerName = textComponent.gameObject.name.ToLowerInvariant();
                if (lowerName.Contains("collect") || lowerName.Contains("trash") || lowerName.Contains("counter"))
                    return textComponent;

                if (fallback == null && textComponent.transform.IsChildOf(transform))
                    fallback = textComponent;
            }

            return fallback;
        }
        catch
        {
            return null;
        }
    }

    private TMP_Text CreateCounterText()
    {
        try
        {
            if (targetCanvas == null)
                targetCanvas = FindAnyObjectByType<Canvas>();

            if (targetCanvas == null)
                return null;

            var counterObject = new GameObject("CollectedText", typeof(RectTransform));
            counterObject.layer = targetCanvas.gameObject.layer;

            var rectTransform = counterObject.GetComponent<RectTransform>();
            rectTransform.SetParent(targetCanvas.transform, false);
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0f, 1f);
            rectTransform.anchoredPosition = new Vector2(40f, -40f);
            rectTransform.sizeDelta = new Vector2(420f, 80f);

            var textComponent = counterObject.AddComponent<TextMeshProUGUI>();
            textComponent.fontSize = 40f;
            textComponent.alignment = TextAlignmentOptions.TopLeft;
            textComponent.color = Color.white;
            textComponent.enableWordWrapping = false;
            textComponent.raycastTarget = false;

            if (TMP_Settings.defaultFontAsset != null)
                textComponent.font = TMP_Settings.defaultFontAsset;

            return textComponent;
        }
        catch
        {
            return null;
        }
    }

    private void UpdateCounter()
    {
        try
        {
            if (counterText == null)
                return;

            counterText.text = string.Format(counterFormat, collectedTrashCount, totalTrashCount);
        }
        catch
        {
        }
    }

    private static int CountTrashObjects()
    {
        try
        {
            return GameObject.FindGameObjectsWithTag("Trash").Length;
        }
        catch
        {
            return 0;
        }
    }
}
