using UnityEngine;
using UnityEngine.UI;

public class QuestButton : MonoBehaviour
{
    [SerializeField] private string requiredQuestID;
    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();
        UpdateButtonState();
    }

    // Call this from NPC after AcceptQuest / HandInQuest so the button reacts
    public void UpdateButtonState()
    {
        if (string.IsNullOrEmpty(requiredQuestID) || QuestController.Instance == null)
        {
            gameObject.SetActive(false);
            return;
        }

        bool questActive = QuestController.Instance.IsQuestActive(requiredQuestID)
                        && !QuestController.Instance.IsQuestHandedIn(requiredQuestID);

        gameObject.SetActive(questActive);
    }
}