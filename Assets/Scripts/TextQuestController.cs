using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextQuestController : MonoBehaviour
{
    private enum NodeResult
    {
        None,
        Dead,
        Win
    }

    [System.Serializable]
    private class QuestChoice
    {
        public string text;
        public int nextNodeIndex;
    }

    [System.Serializable]
    private class QuestNode
    {
        public string title;

        [TextArea(3, 6)]
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
    [SerializeField] private QuestNode[] nodes;

    private void Awake()
    {
        ResolveReferences();

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartQuest);
            restartButton.onClick.AddListener(RestartQuest);
        }
    }

    private void Start()
    {
        if (!HasValidReferences())
            return;

        if (nodes == null || nodes.Length == 0)
            nodes = CreateDefaultNodes();

        buttonTemplate.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        ShowNode(0);
    }

    private void OnDestroy()
    {
        if (restartButton != null)
            restartButton.onClick.RemoveListener(RestartQuest);
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

        if (node.result != NodeResult.None)
        {
            restartButton.gameObject.SetActive(true);
            return;
        }

        if (node.choices == null || node.choices.Length == 0)
            return;

        for (int i = 0; i < node.choices.Length; i++)
        {
            QuestChoice choice = node.choices[i];
            if (choice == null)
                continue;

            Button choiceButton = Instantiate(buttonTemplate, choicesContainer);
            choiceButton.gameObject.SetActive(true);

            TMP_Text label = choiceButton.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
                label.text = choice.text;

            int nextNodeIndex = choice.nextNodeIndex;
            choiceButton.onClick.RemoveAllListeners();
            choiceButton.onClick.AddListener(() => ShowNode(nextNodeIndex));
        }
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
        ShowNode(0);
    }

    private QuestNode[] CreateDefaultNodes()
    {
        return new[]
        {
            new QuestNode
            {
                title = "RECOVERY LOG",
                description = "A weak signal crackles through the trash storms. It comes from the old house uphill.",
                choices = new[]
                {
                    new QuestChoice { text = "Roll closer", nextNodeIndex = 1 }
                }
            },
            new QuestNode
            {
                title = "FRONT YARD",
                description = "The yard is buried in cans, broken plastic, and dead branches. Your scanner finds a power cable under the mess.",
                choices = new[]
                {
                    new QuestChoice { text = "Clear the trash", nextNodeIndex = 2 },
                    new QuestChoice { text = "Force the door", nextNodeIndex = 3 }
                }
            },
            new QuestNode
            {
                title = "FIRST LIGHT",
                description = "You pull the junk away. A garden lamp flickers on. The path to the house glows warm for the first time in years.",
                choices = new[]
                {
                    new QuestChoice { text = "Go to the door", nextNodeIndex = 4 }
                }
            },
            new QuestNode
            {
                title = "SYSTEM FAILURE",
                description = "You force the door. A live wire snaps under the trash. Your systems go dark.",
                result = NodeResult.Dead
            },
            new QuestNode
            {
                title = "INSIDE THE HOUSE",
                description = "Dust covers everything. On a table sits a dead planter unit and a blinking terminal asking for power.",
                choices = new[]
                {
                    new QuestChoice { text = "Power the terminal", nextNodeIndex = 5 },
                    new QuestChoice { text = "Inspect the basement", nextNodeIndex = 6 }
                }
            },
            new QuestNode
            {
                title = "MEMORY LOG",
                description = "The terminal boots. A final log appears: 'If anyone still works out there... help this world breathe again.' A seed compartment unlocks.",
                choices = new[]
                {
                    new QuestChoice { text = "Take the seed pod", nextNodeIndex = 7 }
                }
            },
            new QuestNode
            {
                title = "SYSTEM FAILURE",
                description = "The basement floor gives way beneath you. Rust, darkness, silence.",
                result = NodeResult.Dead
            },
            new QuestNode
            {
                title = "FINAL PATCH",
                description = "Outside, you find one clean patch of soil beside the house. The air is still. Your claws open around the seed pod.",
                choices = new[]
                {
                    new QuestChoice { text = "Plant the seed", nextNodeIndex = 8 },
                    new QuestChoice { text = "Crush it by mistake", nextNodeIndex = 9 }
                }
            },
            new QuestNode
            {
                title = "RECOVERY COMPLETE",
                description = "You press the seed into the soil. By morning, one small green shoot stands against the waste.",
                result = NodeResult.Win
            },
            new QuestNode
            {
                title = "SYSTEM FAILURE",
                description = "The shell cracks in your grip. Nothing grows.",
                result = NodeResult.Dead
            }
        };
    }
}
