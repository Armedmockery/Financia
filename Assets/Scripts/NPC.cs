using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPC : MonoBehaviour, IInteractable
{
    public NPCDialogue dialogueData;
    private DialougeController dialogueUI;
    private int dialogueIndex;
    private bool isTyping, isDialogueActive;
    [SerializeField] public string ID;

    private bool isInfoOpen;
    private DialogueInfo currentInfo;

    private enum QuestState { NotStarted, InProgress, Completed};
    AudioSource voiceSource;
    float voiceTimer;
    private QuestState questState= QuestState.NotStarted; 

    private void Start()
    {
        dialogueUI = DialougeController.instance;
        voiceSource = GetComponent<AudioSource>();
    }
    public bool canInterac()
    {
        return !isDialogueActive;
    }

    public void Interact()
    {
        //if (dialogueData == null || !isDialogueActive)
        //{
        //    return;
        //}
        if (isInfoOpen)
        {
            ToggleInfo();   // closes the panel, sets isInfoOpen = false
            NextLine();     // advances dialogue — this was the missing step
            return;
        }


        if (isDialogueActive)
        {
            NextLine();
        }

        else
        {
            StartDialogue();
            
        }

        
    }

    void StartDialogue()
    {
        var currentEntry = GetCurrentQuestEntry();
        bool hasQuest = currentEntry != null;

        if (hasQuest)
        {
            SyncQuestState();
            if (questState == QuestState.NotStarted)
                dialogueIndex = currentEntry.notStartedDialogueIndex;
            else if (questState == QuestState.InProgress)
                dialogueIndex = currentEntry.inProgressDialogueIndex;
            else if (questState == QuestState.Completed)
                dialogueIndex = currentEntry.completedDialogueIndex;
        }
        else
        {
            dialogueIndex = 0;
        }

        isDialogueActive = true;
        dialogueUI.SetActiveNPC(this);   // 🔥 THIS IS THE KEY LINE

        //PauseController.SetPause(isDialogueActive); //pasue game on dialog open
        dialogueUI.showDialougeUI(true);
        dialogueUI.SetNPCInfo(dialogueData.npcName, dialogueData.npcPotrait);
        PauseController.SetPause(true);
        DisplayCurrentLine();
        

    }

    private QuestSequenceEntry GetCurrentQuestEntry()
    {
        if (dialogueData.questSequence == null || dialogueData.questSequence.Length == 0)
            return null;

        // Walk the sequence — current quest is the first one not yet handed in
        foreach (var entry in dialogueData.questSequence)
        {
            if (!QuestController.Instance.IsQuestHandedIn(entry.quest.questID))
                return entry;
        }

        // All quests handed in — return the last entry so final dialogue still shows
        return dialogueData.questSequence[dialogueData.questSequence.Length - 1];
    }

    private void SyncQuestState()
    {
        //if (dialogueData.quest == null) return;

        var currentEntry = GetCurrentQuestEntry();
        if (currentEntry == null) return;
        string questID = currentEntry.quest.questID;
        Debug.Log($"=== SyncQuestState for {questID} ===");
        Debug.Log($"IsQuestHandedIn: {QuestController.Instance.IsQuestHandedIn(questID)}");
        Debug.Log($"IsQuestActive: {QuestController.Instance.IsQuestActive(questID)}");
        Debug.Log($"IsQuestCompleted: {QuestController.Instance.IsQuestCompleted(questID)}");
        //handing in 
        if (QuestController.Instance.IsQuestHandedIn(questID))
        {
            questState = QuestState.Completed;
            Debug.Log($"{questState} {questID}. Handede in ");
            return;
        }

        if(QuestController.Instance.IsQuestActive(questID))
        {
            if (QuestController.Instance.IsQuestCompleted(questID))
            {
                questState = QuestState.Completed;
                Debug.Log($"{questState} {questID}. Please hand in ");
                EndDialogue();
            }
            else
            {
                questState = QuestState.InProgress;
                Debug.Log($"{questState} {questID}");
            }
        }
        else
        {
            questState = QuestState.NotStarted;
            Debug.Log($"{questState} {questID}");
        }
    }

    void NextLine()
    {
        Debug.Log($"=== NextLine() called. dialogueIndex: {dialogueIndex}, Total lines: {dialogueData.dialogueLines.Length} ===");
        Debug.Log($"endDialogueLines length: {dialogueData.endDialogueLines?.Length ?? 0}");
        if (isTyping)
        {
            StopAllCoroutines();
            voiceSource.Stop();
            dialogueUI.SetDialogueText(dialogueData.dialogueLines[dialogueIndex]);
            isTyping = false;
            
        }

        dialogueUI.ClearChoices();

        CheckForInfo();

        //if(dialogueData.endDialogueLines[dialogueIndex] && dialogueIndex >= (dialogueData.dialogueLines.Length - 1))
        //{
        //    EndDialogue();
        //}
        if(dialogueData.endDialogueLines != null && dialogueData.endDialogueLines.Length > dialogueIndex && dialogueData.endDialogueLines[dialogueIndex])
        {
            Debug.Log($"endDialogueLines[{dialogueIndex}] = true, calling EndDialogue()");
            EndDialogue();
            return;
        }

        foreach(DialogueChoice dialogueChoice in dialogueData.choices)
        {
            if(dialogueChoice.dialogueIndex == dialogueIndex)
            {
                Debug.Log($"Found choice at line {dialogueIndex}, displaying choices");
                DisplayChoices(dialogueChoice);
                return;
            }
        }

        if(++dialogueIndex < dialogueData.dialogueLines.Length)//after adding 1 do we have more line to be typed of showed
        {
            
            DisplayCurrentLine();
        }
        else
        {
            EndDialogue();
        }
    }

    IEnumerator TypeLine()
    {
        Debug.Log($"TypeLine started for line {dialogueIndex}: '{dialogueData.dialogueLines[dialogueIndex]}'");
        isTyping = true;
        dialogueUI.SetDialogueText("");

        foreach (char letter in dialogueData.dialogueLines[dialogueIndex])
        {
            dialogueUI.SetDialogueText(dialogueUI.dialogueText.text += letter);
            if (letter != ' ')
            {
                voiceTimer -= Time.deltaTime;

                if (voiceTimer <= 0)
                {
                    voiceSource.pitch = dialogueData.voicePitch;
                    voiceSource.PlayOneShot(dialogueData.voicSound);
                    voiceTimer = 0.2f; // how fast blips repeat
                }
            }

            yield return new WaitForSeconds(dialogueData.typingSpeed);
        }

        isTyping= false;

        if(dialogueData.autoProgressLines.Length > dialogueIndex && dialogueData.autoProgressLines[dialogueIndex])//if the line is null or not and is not null is it autoprogress and if yes then how its mechanism works
        {
            Debug.Log($"Auto-progressing from line {dialogueIndex}. Waiting {dialogueData.autoProgressDelay} seconds...");
            yield return new WaitForSeconds(dialogueData.autoProgressDelay);
            if (!isInfoOpen)
            {
                NextLine();
            }
        }

    }
    void DisplayChoices(DialogueChoice choice)
    {
        for(int i = 0; i < choice.choices.Length; i++)
        {
            int nextIndex = choice.nextDialogueIndexes[i];
            bool givesQuest=choice.givesQuest[i];
            dialogueUI.CreateChoiceButton(choice.choices[i], () => ChooseOption(nextIndex,givesQuest));

        }
    }

    void ChooseOption(int nextIndex, bool givesQuest)
    {
        if(givesQuest){
            Debug.Log("Accepting Quest");
            var currentEntry = GetCurrentQuestEntry();
            if (currentEntry != null)
            {
                QuestController.Instance.AcceptQuest(currentEntry.quest);
                ItemPickupUIController.Instance?.ShowItemPickup($"New Quest: {currentEntry.quest.questName}");
                NotificationManager.Instance?.Notify();

                // Refresh any QuestButtons in the scene listening to this quest
                foreach (var qb in FindObjectsByType<QuestButton>(FindObjectsSortMode.None))
                    qb.UpdateButtonState();  // ← add this
            }
                
            //QuestController.Instance.EnableQuestObjects(dialogueData.quest.questID.ToString());
            questState =QuestState.InProgress;
            SyncQuestState();
        }
        dialogueIndex = nextIndex;
        dialogueUI.ClearChoices();
        // Check if the new index is valid
        if (dialogueIndex < dialogueData.dialogueLines.Length)
        {
            DisplayCurrentLine();
        }
        else
        {
            // If the next index is beyond the array, end the dialogue
            EndDialogue();
        }
    }

    void DisplayCurrentLine()
    {
        StopAllCoroutines();
        StartCoroutine(TypeLine());
    }

    public void EndDialogue()
    {
        Debug.Log("Calling end dialog");
        // Automatically hand in if quest is completed dialogueData.quest != null &&
        var currentEntry = GetCurrentQuestEntry();
        if (currentEntry != null &&
            questState == QuestState.Completed &&
            !QuestController.Instance.IsQuestHandedIn(currentEntry.quest.questID))
        {
            foreach (var qb in FindObjectsByType<QuestButton>(FindObjectsSortMode.None))
                qb.UpdateButtonState();
            HandleQuestCompletion(currentEntry.quest);
        }

        StopAllCoroutines();
        QuestController.Instance?.UpdateNPCTalked(ID.ToString());
        isDialogueActive = false;

        dialogueUI.ClearChoices();
        dialogueUI.SetDialogueText("");

        dialogueUI.showDialougeUI(false);

        PauseController.SetPause(false);

        dialogueIndex = 0;
        isInfoOpen = false;
        dialogueUI.HideInfoOverlay();
        dialogueUI.ShowInfoButton(false);
    }

    void HandleQuestCompletion(Quest quest)
    {
        if (quest == null)
        {
            Debug.LogError("Quest is null in HandleQuestCompletion!");
            return;
        }

        //Debug.Log($"Calling GiveQuestReward for: {quest.questID}");
        //RewardsController.Instance.GiveQuestReward(quest);

        Debug.Log($"Calling HandInQuest for: {quest.questID}");
        QuestController.Instance.HandInQuest(quest.questID);
        ItemPickupUIController.Instance?.ShowItemPickup($"Quest Complete: {quest.questName}");
        //SyncQuestState();
    }

    public void PlayFootstep()
    {
        SoundEffectManager.Instance.PlayWorldSound("Footstep", transform.position);
    }

    public void ForceEndDialogue()
    {
        
        StopAllCoroutines();
        // 🔥 Reset Info State
        isInfoOpen = false;
        currentInfo = null;
        isDialogueActive = false;

        if (dialogueUI != null)
        {
            dialogueUI.HideInfoOverlay();
            dialogueUI.ShowInfoButton(false);
        }
        QuestController.Instance?.UpdateNPCTalked(ID.ToString());
    }
    void CheckForInfo()
    {
        currentInfo = null;
        dialogueUI.ShowInfoButton(false);

        if (dialogueData.infoLines == null)
            return;

        foreach (var info in dialogueData.infoLines)
        {
            if (info.dialogueIndex == dialogueIndex)
            {
                currentInfo = info;
                dialogueUI.ShowInfoButton(true);
                return;
            }
        }
    }
    public void ToggleInfo()
    {
        if (currentInfo == null)
            return;

        if (!isInfoOpen)
        {
            // Finish typing instantly if mid-typing
            if (isTyping)
            {
                StopAllCoroutines();
                dialogueUI.SetDialogueText(dialogueData.dialogueLines[dialogueIndex]);
                isTyping = false;
            }

            isInfoOpen = true;
            dialogueUI.ShowInfoOverlay(currentInfo.infoText);
        }
        else
        {
            isInfoOpen = false;
            dialogueUI.HideInfoOverlay();
        }
    }
}
