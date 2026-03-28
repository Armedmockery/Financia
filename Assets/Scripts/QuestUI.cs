using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuestUI : MonoBehaviour
{
    public static QuestUI Instance { get; private set; }
    [Header("Test Data")]
    public int testQuestAmount;
    public Quest testQuest;

    private List<QuestProgress> testQuests = new List<QuestProgress>();

    [Header("UI References")]
    public Transform questListContent;
    public GameObject questEntryPrefab;
    public GameObject objectiveTextPrefab;

    //comment out if giving bugs 
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {

        for (int i = 0; i < testQuestAmount; i++)
        {
            testQuests.Add(new QuestProgress(testQuest));
        }

        //UpdateQuestUI();
    }

    public void UpdateQuestUI()
    {
        Debug.Log($"Updating Quest UI for {QuestController.Instance.activateQuests.Count} quests");
        // Destroy existing quest entries
        foreach (Transform child in questListContent)
        {
            Destroy(child.gameObject);
        }
        // Check if we have active quests
        if (QuestController.Instance == null || QuestController.Instance.activateQuests.Count == 0)
        {
            Debug.LogWarning("No active quests in QuestController!");
            return;
        }
        // Build quest entries
        foreach (var quest in QuestController.Instance.activateQuests)
        {
            GameObject entry = Instantiate(questEntryPrefab, questListContent);

            TMP_Text questNameText = entry
                .transform
                .Find("QuestNameText")
                .GetComponent<TMP_Text>();

            Transform objectiveList = entry
                .transform
                .Find("ObjectiveList");

            questNameText.text = quest.quest.questName;

            foreach (var objective in quest.objectives)
            {
                GameObject objTextGO = Instantiate(objectiveTextPrefab, objectiveList);
                TMP_Text objText = objTextGO.GetComponent<TMP_Text>();

                objText.text =
                    $"{objective.description} ({objective.currentAmount}/{objective.requiredAmount})";
                Debug.Log($"{objective.description} ({objective.currentAmount}/{objective.requiredAmount})");
                // Example: Collect 5 Potions (0/5)
            }
        }
    }
}
