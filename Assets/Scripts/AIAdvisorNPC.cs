using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// AI-powered advisor NPC powered by GROQ (LLaMA 3).
/// Same dialogue system as before — only API + personality changed.
/// </summary>
public class AIAdvisorNPC : MonoBehaviour, IInteractable
{
    // ── NPC Identity ──────────────────────────────────────────────────────────
    [Header("NPC Identity")]
    public string npcName = "Kai";   // more friendly name
    public Sprite npcPortrait;

    [Header("Voice & Typewriter")]
    public AudioClip voiceClip;
    [Range(0.5f, 2f)] public float voicePitch = 1.05f;
    public float typingSpeed = 0.035f;
    public float voiceBlipInterval = 0.18f;

    // ── GROQ API ──────────────────────────────────────────────────────────────
    [Header("GROQ API")]
    [Tooltip("Get a free key at https://console.groq.com/keys")]
    public string apiKey = "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";

    private const string API_URL = "https://api.groq.com/openai/v1/chat/completions";
    private const string MODEL = "llama-3.1-8b-instant";
    private const int MAX_TOKENS = 100;

    // ── Personality (FRIENDLY COMPANION) ─────────────────────────────────────
    [Header("Personality")]
    [TextArea(6, 14)]
    public string systemPrompt =
        "You are Kai — a friendly, chill, slightly witty companion traveling alongside the player in a city-based RPG about financial literacy.\n\n" +

        "You are NOT a mentor, NOT an elder, NOT authoritative.\n" +
        "You speak like a smart friend who has your back — casual, supportive, sometimes playful, never preachy.\n\n" +

        "You notice what the player is doing and give helpful insights naturally, like you're thinking out loud together.\n\n" +

        "CRITICAL FORMAT RULES:\n" +
        "- Split response into 3 or 4 lines using ||\n" +
        "- Each line = one dialogue screen\n" +
        "- Keep tone natural, conversational\n\n" +

        "STRUCTURE:\n" +
        "- Line 1: Casual observation about what the player is doing\n" +
        "- Line 2: Helpful financial insight linked to quest\n" +
        "- Line 3: Another practical or smart tip and a Chill closing thought\n" +
        

        "EXAMPLE:\n" +
        "Yo, looks like you're stacking coins pretty fast.||" +
        "Honestly, saving a bit from each reward goes a long way — it adds up quicker than you'd think.||" +
        "Also, maybe track what you're spending — it's easy to lose coins without noticing.||" +
        "You're doing better than you think, just keep it steady.\n\n" +

        "OUTPUT ONLY the lines separated by ||. Dont follow the examples as it is , make sure the leangth of each line is 15-20 words. ";

    // ── Runtime State ─────────────────────────────────────────────────────────
    private DialougeController dialogueUI;
    private AudioSource voiceSource;

    private string[] generatedLines;
    private int lineIndex;
    private bool isTyping;
    private bool isDialogueActive;
    private bool isLoading;

    private float voiceTimer;

    private static readonly string[] LoadingLines =
    {
        "...",
        "Thinking...",
        "Give me a sec..."
    };

    private int loadingLineIndex;
    private Coroutine loadingCoroutine;

    private float lastRequestTime = -10f;

    private void Start()
    {
        dialogueUI = DialougeController.instance;
        voiceSource = GetComponent<AudioSource>();
    }

    public bool canInterac() => !isLoading;

    public void Interact()
    {
        if (isLoading) return;

        if (!isDialogueActive)
        {
            BeginSession();
            return;
        }

        if (isTyping)
        {
            SkipTyping();
            return;
        }

        NextLine();
    }

    void BeginSession()
    {
        isDialogueActive = true;
        isLoading = true;

        dialogueUI.SetActiveNPC(null);
        dialogueUI.showDialougeUI(true);
        dialogueUI.SetNPCInfo(npcName, npcPortrait);
        dialogueUI.ClearChoices();
        dialogueUI.ShowInfoButton(false);
        PauseController.SetPause(true);

        loadingCoroutine = StartCoroutine(AnimateLoading());
        StartCoroutine(FetchAndDisplay());
    }

    void EndSession()
    {
        StopAllCoroutines();

        isDialogueActive = false;
        isLoading = false;
        isTyping = false;
        lineIndex = 0;
        generatedLines = null;

        dialogueUI.ClearChoices();
        dialogueUI.SetDialogueText("");
        dialogueUI.showDialougeUI(false);
        PauseController.SetPause(false);
    }

    void NextLine()
    {
        dialogueUI.ClearChoices();

        int next = lineIndex + 1;
        if (next < generatedLines.Length)
        {
            lineIndex = next;
            DisplayCurrentLine();
        }
        else
        {
            ShowEndChoices();
        }
    }

    void DisplayCurrentLine()
    {
        StopAllCoroutines();
        StartCoroutine(TypeLine(generatedLines[lineIndex]));
    }

    void SkipTyping()
    {
        StopAllCoroutines();
        if (voiceSource != null) voiceSource.Stop();
        isTyping = false;

        if (generatedLines != null && lineIndex < generatedLines.Length)
            dialogueUI.SetDialogueText(generatedLines[lineIndex]);
    }

    IEnumerator TypeLine(string line)
    {
        isTyping = true;
        voiceTimer = 0f;
        dialogueUI.SetDialogueText("");

        var sb = new StringBuilder();
        foreach (char letter in line)
        {
            sb.Append(letter);
            dialogueUI.SetDialogueText(sb.ToString());

            if (letter != ' ' && voiceSource != null && voiceClip != null)
            {
                voiceTimer -= Time.deltaTime;
                if (voiceTimer <= 0f)
                {
                    voiceSource.pitch = voicePitch;
                    voiceSource.PlayOneShot(voiceClip);
                    voiceTimer = voiceBlipInterval;
                }
            }

            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        isTyping = false;

        // AUTO PROGRESS AFTER DELAY
        yield return new WaitForSecondsRealtime(1.5f);

        NextLine();
    }

    void ShowEndChoices()
    {
        dialogueUI.ClearChoices();
        dialogueUI.CreateChoiceButton("Ask again", OnAskAgain);
        dialogueUI.CreateChoiceButton("Cool, thanks.", () => EndSession());
    }

    void OnAskAgain()
    {
        if (Time.time - lastRequestTime < 5f)
        {
            Debug.Log("Too fast, wait a bit...");
            return;
        }

        dialogueUI.ClearChoices();
        isLoading = true;
        loadingCoroutine = StartCoroutine(AnimateLoading());
        StartCoroutine(FetchAndDisplay());
    }

    IEnumerator AnimateLoading()
    {
        loadingLineIndex = 0;
        while (isLoading)
        {
            dialogueUI.SetDialogueText(LoadingLines[loadingLineIndex % LoadingLines.Length]);
            loadingLineIndex++;
            yield return new WaitForSecondsRealtime(1.2f);
        }
    }

    IEnumerator FetchAndDisplay()
    {
        lastRequestTime = Time.time;

        string userPrompt = BuildUserPrompt();
        string jsonBody = BuildRequestJson(userPrompt);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        using var request = new UnityWebRequest(API_URL, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
        request.SetRequestHeader("Accept", "application/json");


        yield return request.SendWebRequest();

        if (loadingCoroutine != null) StopCoroutine(loadingCoroutine);
        isLoading = false;

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[AIAdvisorNPC] GROQ ERROR FULL:\n{request.downloadHandler.text}");
            UseErrorFallback();
            yield break;
        }

        string rawText = ParseResponse(request.downloadHandler.text);

        if (string.IsNullOrWhiteSpace(rawText))
        {
            UseErrorFallback();
            yield break;
        }

        generatedLines = ParseIntoLines(rawText);
        lineIndex = 0;

        dialogueUI.ClearChoices();
        DisplayCurrentLine();
    }

    [System.Serializable]
        public class Message
        {
            public string role;
            public string content;
        }

        [System.Serializable]
        public class RequestBody
        {
            public string model;
            public Message[] messages;
            public int max_tokens;
        }

        string BuildRequestJson(string userPrompt)
        {
            RequestBody body = new RequestBody
            {
                model = MODEL,
                max_tokens = MAX_TOKENS,
                messages = new Message[]
                {
                    new Message { role = "system", content = systemPrompt },
                    new Message { role = "user", content = userPrompt }
                }
            };

            return JsonUtility.ToJson(body);
        }

    string ParseResponse(string json)
    {
        const string marker = "\"content\":\"";
        int start = json.IndexOf(marker);
        if (start < 0) return null;

        start += marker.Length;
        int end = start;

        while (end < json.Length)
        {
            if (json[end] == '\\') { end += 2; continue; }
            if (json[end] == '"') break;
            end++;
        }

        return json.Substring(start, end - start)
            .Replace("\\n", "\n")
            .Replace("\\\"", "\"")
            .Replace("\\\\", "\\");
    }

    string[] ParseIntoLines(string raw)
    {
        var parts = raw.Split(new[] { "||" }, System.StringSplitOptions.RemoveEmptyEntries);
        var result = new List<string>();

        foreach (var p in parts)
        {
            string t = p.Trim();
            if (!string.IsNullOrWhiteSpace(t))
                result.Add(t);
        }

        if (result.Count == 0)
            result.Add(raw);

        return result.ToArray();
    }

    void UseErrorFallback()
    {
        generatedLines = new[]
        {
            "Uh… okay that didn’t work 😅",
            "Give me a sec and try again?"
        };
        lineIndex = 0;
        dialogueUI.ClearChoices();
        DisplayCurrentLine();
    }

    string EscapeJson(string s) => s
        .Replace("\\", "\\\\")
        .Replace("\"", "\\\"")
        .Replace("\n", "\\n")
        .Replace("\r", "")
        .Replace("\t", " ");

    string BuildUserPrompt()
    {
        var sb = new StringBuilder();
        var activeQuests = QuestController.Instance?.activateQuests;

        sb.AppendLine("Player quests:");

        if (activeQuests == null || activeQuests.Count == 0)
        {
            sb.AppendLine("Player just started.");
            return sb.ToString();
        }

        foreach (var q in activeQuests)
        {
            sb.AppendLine(q.quest.questName);
        }

        return sb.ToString();
    }
}