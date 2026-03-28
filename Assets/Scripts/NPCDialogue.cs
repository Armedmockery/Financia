using UnityEngine;

[CreateAssetMenu(fileName = "NewNPCDialogue", menuName = "NPC Dialogue")]
public class NPCDialogue : ScriptableObject
{
    public string npcName;
    public Sprite npcPotrait;
    public string[] dialogueLines;
    public bool[] autoProgressLines;
    public bool[] endDialogueLines;//Marks whe dialouge ends 
    public float autoProgressDelay = 1.5f;
    public float typingSpeed = 0.05f;
    public AudioClip voicSound;
    public float voicePitch = 1f;

    public DialogueChoice[] choices;

    public DialogueInfo[] infoLines;

    public QuestSequenceEntry[] questSequence; // Set up each quest in order in the Inspector
}
[System.Serializable]

public class DialogueChoice
{
    public int dialogueIndex;//dialogue lines where choices appear
    public string[] choices;// player response option
    public int[] nextDialogueIndexes;// where choice leads
    public bool[] givesQuest; 
}

[System.Serializable]
public class DialogueInfo
{
    public int dialogueIndex;
    [TextArea(3, 5)]
    public string infoText;
}

[System.Serializable]
public class QuestSequenceEntry
{
    public Quest quest;
    public int notStartedDialogueIndex;   // dialogue line index when this quest hasn't been given yet
    public int inProgressDialogueIndex;   // dialogue line index while quest is active
    public int completedDialogueIndex;    // dialogue line index when quest is done and ready to hand in
}