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

        public Sprite nodeImage;

        public NodeResult result;
        public QuestChoice[] choices;
    }

    [Header("Quest Images Setup")]
    [Tooltip("Used for: Echo Station, The Atrium")]
    [SerializeField] private Sprite imageAtrium;
    [Tooltip("Used for: Intake Chutes, Deep Chute")]
    [SerializeField] private Sprite imageIntakeChutes;
    [Tooltip("Used for: All Crawlspace paths")]
    [SerializeField] private Sprite imageCrawlspace;
    [Tooltip("Used for: Overseer's Booth, Overseer's Desk")]
    [SerializeField] private Sprite imageOverseersBooth;
    [Tooltip("Used for: Primary Core, Systems Nominal (Alert)")]
    [SerializeField] private Sprite imagePrimaryCore;
    [Tooltip("Used for: All Death screens")]
    [SerializeField] private Sprite imageCriticalFailure;
    [Tooltip("Used for: Facility Secured (Victory)")]
    [SerializeField] private Sprite imageVictory;

    [Header("UI References")]
    [SerializeField] private UnityEngine.UI.Image questImageUI;
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

        if (questImageUI == null)
        {
            Transform imageTransform = transform.Find("QuestImage");
            if (imageTransform != null)
                questImageUI = imageTransform.GetComponent<UnityEngine.UI.Image>();
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

        if (questImageUI != null)
        {
            if (node.nodeImage != null)
            {
                questImageUI.sprite = node.nodeImage;
                questImageUI.gameObject.SetActive(true);
            }
            else
            {
                questImageUI.gameObject.SetActive(false);
            }
        }

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
            SetRestartButtonLabel("Continue");
            restartButton.onClick.AddListener(LoadVictoryScene);
            return;
        }

        SetRestartButtonLabel("Try Again");
        restartButton.onClick.AddListener(RestartQuest);
    }

    private void SetRestartButtonLabel(string text)
    {
        if (restartButtonLabel != null)
            restartButtonLabel.text = text;
    }

    private void LoadVictoryScene()
    {
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
            new QuestNode // 0
            {
                title = "ECHO STATION: INBOUND",
                description = "The airlock hisses, spitting stale air and the scent of ozone. Before you lies Sector 4. The conveyors are frozen like petrified serpents. The facility is dead, but it can be awakened.",
                nodeImage = imageAtrium,
                choices = new[]
                {
                    new QuestChoice { text = "Step into the darkness", nextNodeIndex = 1 }
                }
            },
            new QuestNode // 1
            {
                title = "THE ATRIUM",
                description = "You stand on the main floor. The paths branch out. Above the main door to the Core, a giant neon sign flashes: 'DO NOT USE STANDARD IGNITION (LEFT). IT IS FRIED. USE AUXILIARY (RIGHT) ONLY.' Where to?",
                nodeImage = imageAtrium,
                choices = new[]
                {
                    new QuestChoice { text = "Search the Intake Chutes", nextNodeIndex = 2 },
                    new QuestChoice { text = "Enter the Maintenance Crawlspace", nextNodeIndex = 3 },
                    new QuestChoice { text = "Access the Overseer's Booth", nextNodeIndex = 6 },
                    new QuestChoice { text = "Approach the Primary Core", nextNodeIndex = 8 }
                }
            },
            new QuestNode // 2
            {
                title = "INTAKE CHUTES",
                description = "Mountains of unsorted scrap loom in the dark. Under a collapsed sorting drone, you spot a datapad glowing faintly: 'CRITICAL: The ignition coil will detonate if you route power through the Red circuit first.'",
                nodeImage = imageIntakeChutes,
                choices = new[]
                {
                    new QuestChoice { text = "Dig deeper into the scrap pile", nextNodeIndex = 12 },
                    new QuestChoice { text = "Return to The Atrium", nextNodeIndex = 1 }
                }
            },
            new QuestNode // 3
            {
                title = "MAINTENANCE CRAWLSPACE",
                description = "The ventilation shaft is suffocatingly tight, smelling of burnt copper. The shaft splits ahead into two directions.",
                nodeImage = imageCrawlspace,
                choices = new[]
                {
                    new QuestChoice { text = "Take the left passage", nextNodeIndex = 4 },
                    new QuestChoice { text = "Take the right passage", nextNodeIndex = 5 },
                    new QuestChoice { text = "Back out to The Atrium", nextNodeIndex = 1 }
                }
            },
            new QuestNode // 4
            {
                title = "CRAWLSPACE - LEFT",
                description = "Etched violently into the metal wall with a laser cutter is a frantic message: 'THE BLUE LINE BLEEDS HEAT. IT ALWAYS GOES LAST. IF NOT, WE ALL BURN.'",
                nodeImage = imageCrawlspace,
                choices = new[]
                {
                    new QuestChoice { text = "Return to the junction", nextNodeIndex = 3 }
                }
            },
            new QuestNode // 5
            {
                title = "CRAWLSPACE - RIGHT",
                description = "You find a skeleton in mechanics gear clutching a wrench. A faded manual page in its pocket reads: 'COOLANT PRESSURE WARNING: Valve Alpha opens the main flow. Always start with Alpha to prevent backflow.'",
                nodeImage = imageCrawlspace,
                choices = new[]
                {
                    new QuestChoice { text = "Return to the junction", nextNodeIndex = 3 }
                }
            },
            new QuestNode // 6
            {
                title = "OVERSEER'S BOOTH",
                description = "Shattered glass crunches beneath your boots. The terminal flickers. A corrupted audio log stutters: '...standard overrides... Yellow, Red, Blue... sequence is... *static*...'",
                nodeImage = imageOverseersBooth,
                choices = new[]
                {
                    new QuestChoice { text = "Search the Overseer's desk", nextNodeIndex = 7 },
                    new QuestChoice { text = "Head back down to The Atrium", nextNodeIndex = 1 }
                }
            },
            new QuestNode // 7
            {
                title = "OVERSEER'S DESK",
                description = "Rummaging through the drawers, you find a sticky note attached to a flask: 'To stabilize the core after restart, finally cycle Valve Gamma to regulate the excess pressure. - Supervisor'",
                nodeImage = imageOverseersBooth,
                choices = new[]
                {
                    new QuestChoice { text = "Head back down to The Atrium", nextNodeIndex = 1 }
                }
            },
            new QuestNode // 8
            {
                title = "PRIMARY CORE",
                description = "The main reactor towers before you. A holographic prompt awaits a manual override sequence. Three heavy routing cables lay on the deck: Yellow, Red, and Blue. One wrong move, and the core goes critical.",
                nodeImage = imagePrimaryCore,
                choices = new[]
                {
                    new QuestChoice { text = "Step back. I need to check the logs again.", nextNodeIndex = 1 },
                    new QuestChoice { text = "Connect Red -> Yellow -> Blue", nextNodeIndex = 9 },
                    new QuestChoice { text = "Connect Yellow -> Blue -> Red", nextNodeIndex = 10 },
                    new QuestChoice { text = "Connect Yellow -> Red -> Blue", nextNodeIndex = 11 }
                }
            },
            new QuestNode // 9
            {
                title = "CATASTROPHIC FEEDBACK",
                description = "You slide the Red cable in first. A high-pitched whine pierces your ears. The ignition coil overloads instantly, engulfing the room in searing white plasma. You were warned.",
                nodeImage = imageCriticalFailure,
                result = NodeResult.End
            },
            new QuestNode // 10
            {
                title = "THERMAL RUNAWAY",
                description = "You connect the Blue line too early. The cooling loops fail to sync. Alarms shriek as heat bleeds uncontrollably through the chassis. The metal melts beneath your feet.",
                nodeImage = imageCriticalFailure,
                result = NodeResult.End
            },
            new QuestNode // 11
            {
                title = "SYSTEMS NOMINAL... ALERT!",
                description = "Yellow clicks in. Red pre-warms. Blue cools. The core hums to life! But suddenly, crimson sirens blare!\n'WARNING: COOLANT PRESSURE CRITICAL. MANUAL VALVE OVERRIDE REQUIRED.'\nYou rush to the Coolant Terminal.",
                nodeImage = imagePrimaryCore,
                choices = new[]
                {
                    new QuestChoice { text = "Open Valve Beta, then Gamma", nextNodeIndex = 13 },
                    new QuestChoice { text = "Open Valve Gamma, then Alpha", nextNodeIndex = 14 },
                    new QuestChoice { text = "Open Valve Alpha, then Gamma", nextNodeIndex = 15 }
                }
            },
            new QuestNode // 12
            {
                title = "DEEP CHUTE",
                description = "You squeeze further into the debris and find a jammed pneumatic lock with a scribbled warning: 'Secondary Coolant: Valve Beta seal is blown. Do NOT attempt to cycle Beta or we flood the chamber.'",
                nodeImage = imageIntakeChutes,
                choices = new[]
                {
                    new QuestChoice { text = "Return to The Atrium", nextNodeIndex = 1 }
                }
            },
            new QuestNode // 13
            {
                title = "TOXIC FLOOD",
                description = "You initiate Valve Beta. The blown seal gives way completely, flooding the chamber with supercooled liquid nitrogen. You are frozen instantly where you stand.",
                nodeImage = imageCriticalFailure,
                result = NodeResult.End
            },
            new QuestNode // 14
            {
                title = "PRESSURE INVERSION",
                description = "You open Valve Gamma before releasing the main flow from Alpha. The pressure difference crushes the pipes inward, causing a massive internal explosion.",
                nodeImage = imageCriticalFailure,
                result = NodeResult.End
            },
            new QuestNode // 15
            {
                title = "PRESSURE STABILIZED... STANDBY",
                description = "Valve Alpha opens the flow, and Gamma regulates the pressure! The alarms stop, but a final Igniter Lever unlocks. Beside it is a Safety Mode Dial.",
                nodeImage = imagePrimaryCore,
                choices = new[]
                {
                    new QuestChoice { text = "Turn Dial RIGHT and Ignite", nextNodeIndex = 17 },
                    new QuestChoice { text = "Turn Dial LEFT and Ignite", nextNodeIndex = 16 }
                }
            },
            new QuestNode // 16
            {
                title = "FRIED RELAYS",
                description = "You turn it LEFT to Standard. The broken relays instantly short-circuit. Millions of volts arc through the console, electrocuting you on the spot.",
                nodeImage = imageCriticalFailure,
                result = NodeResult.End
            },
            new QuestNode // 17
            {
                title = "FACILITY SECURED",
                description = "You turn the dial RIGHT to Auxiliary and pull the lever. The core ignites with a safe, steady roar! Warm lights flood the facility. The recycler is fixed and you survive!",
                nodeImage = imageVictory,
                result = NodeResult.Victory
            }
        };
    }
}
