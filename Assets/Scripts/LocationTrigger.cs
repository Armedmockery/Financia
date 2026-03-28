using UnityEngine;

public class LocationTrigger : MonoBehaviour
{
    [Header("Location Identifier")]
    public string locationID;          // e.g., "ForestEntrance", "Cave"

    [Header("Quest Association")]
    public string requiredQuestID;

    private void Start()
    {
        if (SaveController.Instance != null &&
            SaveController.Instance.GetVisitCount(locationID) > 0)
        {

            gameObject.SetActive(false);
            return;
        }
        if (!string.IsNullOrEmpty(requiredQuestID))
        {
            bool questActive = QuestController.Instance != null &&
                               QuestController.Instance.IsQuestActive(requiredQuestID);
            gameObject.SetActive(true);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Notify quest system that player entered this location
        if (QuestController.Instance != null)
        {
            QuestController.Instance.UpdateLocationVisit(locationID, 1);
            SaveController.Instance?.AddVisitedLocation(locationID);
            gameObject.SetActive(false);
        }
    }
}