// using UnityEngine;
// using TMPro;
// using UnityEngine.UI;
// using System;

// public class MazeDialogUI : MonoBehaviour
// {
//     public static MazeDialogUI Instance;

//     public GameObject panel;
//     public TMP_Text messageText;

//     public Button continueButton;
//     public Button retryButton;
//     public Button returnButton;

//     private Action continueAction;
//     private Action retryAction;
//     private Action returnAction;

//     private void Awake()
//     {
//         Instance = this;
//         panel.SetActive(false);
//     }

//     public void ShowMessage(string message, Action onContinue)
//     {
//         panel.SetActive(true);

//         messageText.text = message;

//         continueButton.gameObject.SetActive(true);
//         retryButton.gameObject.SetActive(false);
//         returnButton.gameObject.SetActive(false);

//         // Change button text to "Continue"
//         TextMeshProUGUI buttonText = continueButton.GetComponentInChildren<TextMeshProUGUI>();
//         if (buttonText != null)
//             buttonText.text = "Continue";

//         continueAction = onContinue;

//         continueButton.onClick.RemoveAllListeners();
//         continueButton.onClick.AddListener(() =>
//         {
//             panel.SetActive(false);
//             continueAction?.Invoke();
//         });
//     }

//     public void ShowFailureDialog(
//         string message,
//         Action onRetry,
//         Action onReturn)
//     {
//         panel.SetActive(true);

//         messageText.text = message;

//         continueButton.gameObject.SetActive(false);
//         retryButton.gameObject.SetActive(true);
//         returnButton.gameObject.SetActive(true);

//         // Set button texts
//         TextMeshProUGUI retryText = retryButton.GetComponentInChildren<TextMeshProUGUI>();
//         if (retryText != null)
//             retryText.text = "Retry";

//         TextMeshProUGUI returnText = returnButton.GetComponentInChildren<TextMeshProUGUI>();
//         if (returnText != null)
//             returnText.text = "Return to City";

//         retryAction = onRetry;
//         returnAction = onReturn;

//         retryButton.onClick.RemoveAllListeners();
//         retryButton.onClick.AddListener(() =>
//         {
//             panel.SetActive(false);
//             retryAction?.Invoke();
//         });

//         returnButton.onClick.RemoveAllListeners();
//         returnButton.onClick.AddListener(() =>
//         {
//             panel.SetActive(false);
//             returnAction?.Invoke();
//         });
//     }

//     // NEW: Completion dialog showing summary with Continue button
//     public void ShowCompletionDialog(string message, Action onConfirm)
//     {
//         Time.timeScale = 0f;
        
//         panel.SetActive(true);
//         messageText.text = message;

//         // Show only continue button
//         continueButton.gameObject.SetActive(true);
//         retryButton.gameObject.SetActive(false);
//         returnButton.gameObject.SetActive(false);

//         // Change button text to "Continue"
//         TextMeshProUGUI buttonText = continueButton.GetComponentInChildren<TextMeshProUGUI>();
//         if (buttonText != null)
//             buttonText.text = "Continue";

//         continueAction = onConfirm;

//         continueButton.onClick.RemoveAllListeners();
//         continueButton.onClick.AddListener(() =>
//         {
//             Time.timeScale = 1f;
//             panel.SetActive(false);
//             continueAction?.Invoke();
//         });

//         Debug.Log("Showing completion dialog: " + message);
//     }

//     // Optional: Method to hide dialog
//     public void HideDialog()
//     {
//         panel.SetActive(false);
//         Time.timeScale = 1f;
//     }
// }
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class MazeDialogUI : MonoBehaviour
{
    public static MazeDialogUI Instance;

    public GameObject panel;
    public TMP_Text messageText;

    public Button continueButton;
    public Button retryButton;
    public Button returnButton;
    public Button viewJourneyButton;  // NEW

    [Header("Menu Reference")]  // NEW
    public GameObject menuPanel;  // Your main menu with tabs
    public TaxJourneyUI taxJourneyUI;  // Tax Journey page

    private Action continueAction;
    private Action retryAction;
    private Action returnAction;

    private void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void ShowMessage(string message, Action onContinue)
    {
        panel.SetActive(true);
        messageText.text = message;

        continueButton.gameObject.SetActive(true);
        retryButton.gameObject.SetActive(false);
        returnButton.gameObject.SetActive(false);
        viewJourneyButton.gameObject.SetActive(false);

        TextMeshProUGUI buttonText = continueButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null) buttonText.text = "Continue";

        continueAction = onContinue;

        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(() =>
        {
            panel.SetActive(false);
            continueAction?.Invoke();
        });
    }

    // NEW: Completion Dialog
    public void ShowCompletionDialog(int wisdom, int roomsCleared, int totalRooms)
    {
        panel.SetActive(true);
        Time.timeScale = 0f;

        messageText.text = $"✨ MAZE COMPLETE! ✨\n\n" +
                          $"Rooms Cleared: {roomsCleared}/{totalRooms}\n" +
                          $"Wisdom Score: {wisdom}\n\n" +
                          $"Review your tax journey or return to city?";

        continueButton.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(false);
        returnButton.gameObject.SetActive(true);
        viewJourneyButton.gameObject.SetActive(true);

        // Set button texts
        SetButtonText(returnButton, "Return to City");
        SetButtonText(viewJourneyButton, "View Full Journey");

        returnButton.onClick.RemoveAllListeners();
        returnButton.onClick.AddListener(ReturnToCity);

        viewJourneyButton.onClick.RemoveAllListeners();
        viewJourneyButton.onClick.AddListener(OpenTaxJourney);
    }

    // NEW: Game Over Dialog
    public void ShowGameOverDialog(int wisdom, int roomsCleared, int totalRooms)
    {
        panel.SetActive(true);
        Time.timeScale = 0f;

        messageText.text = $" TAX AUDIT FAILED \n\n" +
                          $"Rooms Cleared: {roomsCleared}/{totalRooms}\n" +
                          $"Wisdom Score: {wisdom}\n\n" +
                          $"Review your journey, retry, or return?";

        continueButton.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(true);
        returnButton.gameObject.SetActive(true);
        viewJourneyButton.gameObject.SetActive(true);

        SetButtonText(retryButton, "Retry Maze");
        SetButtonText(returnButton, "Return to City");
        SetButtonText(viewJourneyButton, "View Journey");

        retryButton.onClick.RemoveAllListeners();
        retryButton.onClick.AddListener(RetryMaze);

        returnButton.onClick.RemoveAllListeners();
        returnButton.onClick.AddListener(ReturnToCity);

        viewJourneyButton.onClick.RemoveAllListeners();
        viewJourneyButton.onClick.AddListener(OpenTaxJourney);
    }

    void OpenTaxJourney()
{
    panel.SetActive(false);  // Hide dialog
    menuPanel.SetActive(true);  // Show menu
    taxJourneyUI.gameObject.SetActive(true);  // Show journey page
    taxJourneyUI.RefreshJourney();  // Refresh data
    
    // No need to set button visibility here - TaxJourneyUI handles it
}

    // NEW: Close menu and return to dialog
    public void CloseMenuAndReturnToDialog()
    {
        menuPanel.SetActive(false);
        panel.SetActive(true);
        // Dialog is still there with same state
    }

    // NEW: Retry Maze
    public void RetryMaze()
    {
        Time.timeScale = 1f;
        panel.SetActive(false);
        menuPanel.SetActive(false);
        
        if (MazeLifeManager.Instance != null)
        {
            MazeLifeManager.Instance.RetryMaze();
        }
    }

    // NEW: Return to City
    public void ReturnToCity()
    {
        Time.timeScale = 1f;
        Debug.Log("Returning to City...");
        // SceneManager.LoadScene("CityScene");
    }

    void SetButtonText(Button btn, string text)
    {
        TextMeshProUGUI tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null) tmp.text = text;
    }

    // Keep your existing ShowFailureDialog method
    public void ShowFailureDialog(string message, Action onRetry, Action onReturn)
    {
        panel.SetActive(true);
        messageText.text = message;

        continueButton.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(true);
        returnButton.gameObject.SetActive(true);
        viewJourneyButton.gameObject.SetActive(false);

        SetButtonText(retryButton, "Retry");
        SetButtonText(returnButton, "Return to City");

        retryAction = onRetry;
        returnAction = onReturn;

        retryButton.onClick.RemoveAllListeners();
        retryButton.onClick.AddListener(() =>
        {
            panel.SetActive(false);
            retryAction?.Invoke();
        });

        returnButton.onClick.RemoveAllListeners();
        returnButton.onClick.AddListener(() =>
        {
            panel.SetActive(false);
            returnAction?.Invoke();
        });
    }
}