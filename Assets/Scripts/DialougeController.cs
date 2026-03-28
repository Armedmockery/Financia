using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialougeController : MonoBehaviour
{
    public static DialougeController instance { get; private set; }
    public GameObject dialoguePanel;
    public TMP_Text dialogueText, nameText;
    public Image portraitImage;
    public Transform choiceContainer;
    public GameObject choiceButtonPrefab;

    public GameObject infoButton;
    public GameObject infoOverlayPanel;
    public TMP_Text infoText;
    private NPC activeNPC;
    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        infoButton.GetComponent<Button>().onClick.AddListener(ToggleInfo);

    }

    public void showDialougeUI(bool show)
    {
        dialoguePanel.SetActive(show);
    }

    public void SetNPCInfo(string npcName, Sprite portrait)
    {
        nameText.text = npcName;
        portraitImage.sprite = portrait;
    }

    public void SetDialogueText(string text)
    {
        dialogueText.text = text;
    }

    public void ClearChoices()
    {
        foreach (Transform child in choiceContainer) Destroy(child.gameObject);
    }

    public void CreateChoiceButton(string choiceText , UnityEngine.Events.UnityAction onClick)
    {
        GameObject choiceButton = Instantiate(choiceButtonPrefab , choiceContainer);
        choiceButton.GetComponentInChildren<TMP_Text>().text = choiceText;
        choiceButton.GetComponent<Button>().onClick.AddListener(onClick);
        
    }

    public void SetActiveNPC(NPC npc)
    {
        activeNPC = npc;
    }

    public void CloseDialogue()
    {
        if (activeNPC != null)
        {
            activeNPC.ForceEndDialogue();
            activeNPC = null;
        }

        ClearChoices();
        SetDialogueText("");
        showDialougeUI(false);

        PauseController.SetPause(false);
    }
    public void ShowInfoButton(bool show)
    {
        infoButton.SetActive(show);
    }

    public void ShowInfoOverlay(string text)
    {
        infoOverlayPanel.SetActive(true);
        infoText.text = text;
    }

    public void HideInfoOverlay()
    {
        infoOverlayPanel.SetActive(false);
    }
    void ToggleInfo()
    {
        if (activeNPC != null)
        {
            activeNPC.ToggleInfo();
        }
    }
}