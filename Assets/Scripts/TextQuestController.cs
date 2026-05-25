using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TextQuestController : MonoBehaviour
{
    private enum NodeResult
    {
        None,
        End,
        Victory
    }

    [System.Serializable]
    private class QuestChoice
    {
        public string text;
        public int nextNodeIndex;
        public string[] requiredFlags;
        public string[] requiredAnyFlags;
        public string[] blockedFlags;
        public string[] setFlags;
        public string[] clearFlags;
    }

    [System.Serializable]
    private class QuestNode
    {
        public string title;

        [TextArea(3, 8)]
        public string description;

        public NodeResult result;
        public QuestChoice[] choices;
    }

    [SerializeField] private string defaultTitle = "RECOVERY LOG";
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Transform choicesContainer;
    [SerializeField] private Button buttonTemplate;
    [SerializeField] private Button restartButton;
    [SerializeField] private TMP_Text restartButtonLabel;
    [SerializeField] private QuestNode[] nodes;
    [SerializeField] private string retryButtonText = "Try Again";
    [SerializeField] private string victoryButtonText = "Continue";
    [SerializeField] private string victorySceneName = "FakeGravity";

    private readonly HashSet<string> activeFlags = new();

    private void Awake()
    {
        ResolveReferences();
    }

    private void Start()
    {
        if (!HasValidReferences())
            return;

        if (nodes == null || nodes.Length == 0)
            nodes = CreateDefaultNodes();

        buttonTemplate.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        RestartQuest();
    }

    private void OnDestroy()
    {
        if (restartButton != null)
            restartButton.onClick.RemoveAllListeners();
    }

    [ContextMenu("Use Default Nodes")]
    private void UseDefaultNodes()
    {
        nodes = CreateDefaultNodes();
    }

    private void ResolveReferences()
    {
        if (titleText == null)
        {
            Transform titleTransform = transform.Find("TitleText");
            if (titleTransform != null)
                titleText = titleTransform.GetComponent<TMP_Text>();
        }

        if (descriptionText == null)
        {
            Transform descriptionTransform = transform.Find("DescriptionText");
            if (descriptionTransform != null)
                descriptionText = descriptionTransform.GetComponent<TMP_Text>();
        }

        if (choicesContainer == null)
        {
            Transform choicesTransform = transform.Find("Choices");
            if (choicesTransform != null)
                choicesContainer = choicesTransform;
        }

        if (buttonTemplate == null && choicesContainer != null)
        {
            Transform templateTransform = choicesContainer.Find("ButtonTemplate");
            if (templateTransform != null)
                buttonTemplate = templateTransform.GetComponent<Button>();
        }

        if (restartButton == null)
        {
            Transform restartTransform = transform.Find("RestartButton");
            if (restartTransform != null)
                restartButton = restartTransform.GetComponent<Button>();
        }

        if (restartButtonLabel == null && restartButton != null)
            restartButtonLabel = restartButton.GetComponentInChildren<TMP_Text>(true);
    }

    private bool HasValidReferences()
    {
        bool isValid = titleText != null
            && descriptionText != null
            && choicesContainer != null
            && buttonTemplate != null
            && restartButton != null;

        if (!isValid)
            Debug.LogError("TextQuestController is missing UI references.", this);

        return isValid;
    }

    private void ShowNode(int nodeIndex)
    {
        if (nodes == null || nodes.Length == 0)
            return;

        if (nodeIndex < 0 || nodeIndex >= nodes.Length)
            nodeIndex = 0;

        QuestNode node = nodes[nodeIndex];

        ClearChoiceButtons();
        restartButton.gameObject.SetActive(false);

        titleText.text = string.IsNullOrWhiteSpace(node.title) ? defaultTitle : node.title;
        descriptionText.text = node.description;

        if (node.result == NodeResult.End || node.result == NodeResult.Victory)
        {
            ConfigureEndButton(node.result);
            restartButton.gameObject.SetActive(true);
            return;
        }

        if (node.choices == null || node.choices.Length == 0)
        {
            ConfigureEndButton(NodeResult.End);
            restartButton.gameObject.SetActive(true);
            return;
        }

        int visibleChoices = 0;

        for (int i = 0; i < node.choices.Length; i++)
        {
            QuestChoice choice = node.choices[i];
            if (choice == null || !IsChoiceAvailable(choice))
                continue;

            Button choiceButton = Instantiate(buttonTemplate, choicesContainer);
            choiceButton.gameObject.SetActive(true);

            TMP_Text label = choiceButton.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
                label.text = choice.text;

            QuestChoice cachedChoice = choice;
            choiceButton.onClick.RemoveAllListeners();
            choiceButton.onClick.AddListener(() => SelectChoice(cachedChoice));
            visibleChoices++;
        }

        if (visibleChoices == 0)
        {
            ConfigureEndButton(NodeResult.End);
            restartButton.gameObject.SetActive(true);
        }
    }

    private void ConfigureEndButton(NodeResult result)
    {
        if (restartButton == null)
            return;

        restartButton.onClick.RemoveAllListeners();

        if (result == NodeResult.Victory)
        {
            SetRestartButtonLabel(victoryButtonText);
            restartButton.onClick.AddListener(LoadVictoryScene);
            return;
        }

        SetRestartButtonLabel(retryButtonText);
        restartButton.onClick.AddListener(RestartQuest);
    }

    private void SetRestartButtonLabel(string text)
    {
        if (restartButtonLabel != null)
            restartButtonLabel.text = text;
    }

    private void LoadVictoryScene()
    {
        SceneSessionState.MarkCurrentFacilityCompleted();

        if (string.IsNullOrWhiteSpace(victorySceneName))
        {
            RestartQuest();
            return;
        }

        SceneManager.LoadScene(victorySceneName);
    }


    private bool IsChoiceAvailable(QuestChoice choice)
    {
        if (choice == null)
            return false;

        if (HasAnyFlags(choice.blockedFlags))
            return false;

        if (!HasAllFlags(choice.requiredFlags))
            return false;

        if (choice.requiredAnyFlags != null && choice.requiredAnyFlags.Length > 0 && !HasAnyFlags(choice.requiredAnyFlags))
            return false;

        return true;
    }

    private void SelectChoice(QuestChoice choice)
    {
        ApplyFlagChanges(choice.clearFlags, false);
        ApplyFlagChanges(choice.setFlags, true);
        ShowNode(choice.nextNodeIndex);
    }

    private void ApplyFlagChanges(string[] flags, bool setValue)
    {
        if (flags == null)
            return;

        for (int i = 0; i < flags.Length; i++)
        {
            string flag = flags[i];
            if (string.IsNullOrWhiteSpace(flag))
                continue;

            if (setValue)
                activeFlags.Add(flag);
            else
                activeFlags.Remove(flag);
        }
    }

    private bool HasAllFlags(string[] flags)
    {
        if (flags == null || flags.Length == 0)
            return true;

        for (int i = 0; i < flags.Length; i++)
        {
            string flag = flags[i];
            if (string.IsNullOrWhiteSpace(flag))
                continue;

            if (!activeFlags.Contains(flag))
                return false;
        }

        return true;
    }

    private bool HasAnyFlags(string[] flags)
    {
        if (flags == null || flags.Length == 0)
            return false;

        for (int i = 0; i < flags.Length; i++)
        {
            string flag = flags[i];
            if (string.IsNullOrWhiteSpace(flag))
                continue;

            if (activeFlags.Contains(flag))
                return true;
        }

        return false;
    }

    private void ClearChoiceButtons()
    {
        if (choicesContainer == null)
            return;

        for (int i = choicesContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = choicesContainer.GetChild(i);
            if (buttonTemplate != null && child == buttonTemplate.transform)
                continue;

            Destroy(child.gameObject);
        }
    }

    private void RestartQuest()
    {
        activeFlags.Clear();
        ShowNode(0);
    }

    private QuestNode[] CreateDefaultNodes()
    {
        return new[]
        {
            new QuestNode
            {
                title = "FACILITY PING",
                description = "An abandoned recycling facility wheezes in the dust. Inside, something is broken.",
                choices = new[]
                {
                    new QuestChoice { text = "Go inside", nextNodeIndex = 1 }
                }
            },
            new QuestNode
            {
                title = "MAIN FLOOR",
                description = "Dead belts. Dark panel. Jammed intake. Loose hatch.",
                choices = new[]
                {
                    new QuestChoice
                    {
                        text = "Step on the yellow thing",
                        nextNodeIndex = 2
                    },
                    new QuestChoice
                    {
                        text = "Clear the intake",
                        nextNodeIndex = 3,
                        blockedFlags = new[] { "intake_fixed" }
                    },
                    new QuestChoice
                    {
                        text = "Open the hatch",
                        nextNodeIndex = 4,
                        blockedFlags = new[] { "battery_found" }
                    },
                    new QuestChoice
                    {
                        text = "Kick the panel",
                        nextNodeIndex = 5
                    }
                }
            },
            new QuestNode
            {
                title = "BANANA INCIDENT",
                description = "It was a banana peel. You slip, spin, and crash into the floor dramatically. How did it survive all these years untouched? I guess we will never know.",
                result = NodeResult.End
            },
            new QuestNode
            {
                title = "INTAKE CHUTE",
                description = "You yank out junk and one rubber duck. The intake beeps happily.",
                choices = new[]
                {
                    new QuestChoice
                    {
                        text = "Take the duck",
                        nextNodeIndex = 1,
                        setFlags = new[] { "intake_fixed", "duck" }
                    }
                }
            },
            new QuestNode
            {
                title = "MAINTENANCE HATCH",
                description = "Dust, bolts, and one backup battery. Nice.",
                choices = new[]
                {
                    new QuestChoice
                    {
                        text = "Take the battery",
                        nextNodeIndex = 7,
                        requiredFlags = new[] { "intake_fixed" },
                        setFlags = new[] { "battery_found" }
                    },
                    new QuestChoice
                    {
                        text = "Crawl deeper",
                        nextNodeIndex = 6
                    }
                }
            },
            new QuestNode
            {
                title = "BUTTON CHAOS",
                description = "The panel plays cheerful music and prints: BAD PLAN.",
                result = NodeResult.End
            },
            new QuestNode
            {
                title = "DUCT DETOUR",
                description = "You get lost in a duct and come back wearing a caution sign like a cape.",
                result = NodeResult.End
            },
            new QuestNode
            {
                title = "YOU WIN",
                description = "You slot in the battery. Fans spin. Belts clatter. Green lights blink on. The facility starts sorting trash again. The recycler is fixed.",
                result = NodeResult.Victory
            }
        };
    }
}
