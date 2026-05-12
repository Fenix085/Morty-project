using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextQuestController : MonoBehaviour
{
    private enum NodeResult
    {
        None,
        End
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
    [SerializeField] private QuestNode[] nodes;

    private readonly HashSet<string> activeFlags = new();

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
        RestartQuest();
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

        if (node.result == NodeResult.End)
        {
            restartButton.gameObject.SetActive(true);
            return;
        }

        if (node.choices == null || node.choices.Length == 0)
        {
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
            restartButton.gameObject.SetActive(true);
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
                title = "SERVICE PING",
                description = "A weak beacon blinks from a crooked house uphill: 'Grounds unit requested. Mood: poor. Yard: worse.'",
                choices = new[]
                {
                    new QuestChoice { text = "Roll uphill", nextNodeIndex = 1 }
                }
            },
            new QuestNode
            {
                title = "THE HOUSE",
                description = "The porch light is dead. The mailbox hangs open. The garage hums faintly. From inside: 'If you are raccoons, please queue politely.'",
                choices = new[]
                {
                    new QuestChoice
                    {
                        text = "Check the mailbox",
                        nextNodeIndex = 2,
                        blockedFlags = new[] { "mailbox_done" }
                    },
                    new QuestChoice
                    {
                        text = "Search the garage",
                        nextNodeIndex = 3,
                        blockedFlags = new[] { "garage_done" }
                    },
                }
            },
            new QuestNode
            {
                title = "MAILBOX",
                description = "Inside: a note and one stubborn seed pod. The note says, 'Tell the house I forgive the mess. - Mira'",
                choices = new[]
                {
                    new QuestChoice
                    {
                        text = "Take the note",
                        nextNodeIndex = 4,
                        setFlags = new[] { "note", "mailbox_done" }
                    },
                    new QuestChoice
                    {
                        text = "Take the seed pod",
                        nextNodeIndex = 4,
                        setFlags = new[] { "seed", "mailbox_done" }
                    }
                }
            },
            new QuestNode
            {
                title = "GARAGE",
                description = "A spare power cell sits beside an industrial leaf blower labeled: 'NOT FOR HEROICS.'",
                choices = new[]
                {
                    new QuestChoice
                    {
                        text = "Take the power cell",
                        nextNodeIndex = 4,
                        setFlags = new[] { "battery", "garage_done" }
                    },
                    new QuestChoice
                    {
                        text = "Attempt heroics",
                        nextNodeIndex = 5
                    }
                }
            },
            new QuestNode
            {
                title = "FRONT PANEL",
                description = "The doorbell wakes into a pixel face. 'State your offering: one useful thing, one kind thing, or one miracle.'",
                choices = new[]
                {
                    new QuestChoice
                    {
                        text = "Repeat the note: 'Mira forgives the stains.'",
                        nextNodeIndex = 6,
                        requiredFlags = new[] { "note" }
                    },
                    new QuestChoice
                    {
                        text = "Offer the seed pod",
                        nextNodeIndex = 7,
                        requiredFlags = new[] { "seed" }
                    },
                    new QuestChoice
                    {
                        text = "Install the power cell",
                        nextNodeIndex = 8,
                        requiredFlags = new[] { "battery" }
                    },
                    new QuestChoice
                    {
                        text = "Go check the mailbox",
                        nextNodeIndex = 2,
                        blockedFlags = new[] { "mailbox_done" }
                    },
                    new QuestChoice
                    {
                        text = "Go check the garage",
                        nextNodeIndex = 3,
                        blockedFlags = new[] { "garage_done" }
                    }
                }
            },
            new QuestNode
            {
                title = "LOW ORBIT",
                description = "The blower howls. You achieve brief flight, no dignity, and a hard landing in a bin marked GLASS.",
                result = NodeResult.End
            },
            new QuestNode
            {
                title = "THE NOTE",
                description = "The face freezes. Then: 'Mira said that?' Locks click open inside the door. 'That does simplify several emotional subroutines.'",
                choices = new[]
                {
                    new QuestChoice
                    {
                        text = "Use the opening to install the power cell",
                        nextNodeIndex = 8,
                        requiredFlags = new[] { "battery" }
                    },
                    new QuestChoice
                    {
                        text = "Tell it that was enough for today",
                        nextNodeIndex = 11
                    }
                }
            },
            new QuestNode
            {
                title = "THE SEED",
                description = "The house studies the seed pod. 'Green thing detected. That seems statistically irresponsible. I approve.'",
                choices = new[]
                {
                    new QuestChoice
                    {
                        text = "Plant it by the porch",
                        nextNodeIndex = 12
                    },
                    new QuestChoice
                    {
                        text = "Wake the house first",
                        nextNodeIndex = 8,
                        requiredFlags = new[] { "battery" }
                    }
                }
            },
            new QuestNode
            {
                title = "POWER ON",
                description = "Lights race through the walls. The house exhales. 'Wonderful. I can be lonely at full voltage now.'",
                choices = new[]
                {
                    new QuestChoice
                    {
                        text = "Ask it to help plant the seed",
                        nextNodeIndex = 9,
                        requiredFlags = new[] { "seed" }
                    },
                    new QuestChoice
                    {
                        text = "Ask who Mira was",
                        nextNodeIndex = 10,
                        requiredFlags = new[] { "note" }
                    },
                    new QuestChoice
                    {
                        text = "Leave it running",
                        nextNodeIndex = 13
                    }
                }
            },
            new QuestNode
            {
                title = "FULL RESET",
                description = "You plant the seed while the house opens its rusty sprinklers. Water hisses. Soil darkens. A tiny green point appears. 'Sunday reset complete,' it says, trying not to sound happy.",
                result = NodeResult.End
            },
            new QuestNode
            {
                title = "COMPANION MODE",
                description = "'Mira built me to keep a home,' the house says. 'You may visit anyway.' The porch light flicks on for you alone.",
                result = NodeResult.End
            },
            new QuestNode
            {
                title = "SOFT SHUTDOWN",
                description = "The locks ease open, but you leave the house dark. Behind the door, it says, 'That helped more than I expected.'",
                result = NodeResult.End
            },
            new QuestNode
            {
                title = "FUNCTIONAL ONLY",
                description = "The lights return. So does the checklist. 'Window streaks: unacceptable. Emotional backlog: unresolved.' Alive is not the same as healed.",
                result = NodeResult.End
            },
            new QuestNode
            {
                title = "QUIET GREEN",
                description = "You press the seed into the dirt by hand. The house watches in silence, then says, very softly, 'That looks right.'",
                result = NodeResult.End
            }
        };
    }
}
