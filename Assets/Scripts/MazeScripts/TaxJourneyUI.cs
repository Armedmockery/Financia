
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class TaxJourneyUI : MonoBehaviour
{
    [Header("Content Parents")]
    public Transform contentParent;           // ScrollView Content
    public Transform rewardGridContainer;     // Grid container (child of contentParent)
    
    [Header("Prefabs")]
    public GameObject choiceItemPrefab;
    public GameObject rewardItemPrefab;
    
    [Header("Headers")]
    public GameObject rewardsHeaderPrefab;     // "REWARDS COLLECTED" text
    
    [Header("Buttons")]
    public Button retryMazeButton;
    public Button returnToCityButton;
    public Button closeButton;
    
    [Header("References")]
    public MazeDialogUI mazeDialog;
    
    [Header("Grid Settings")]
    public int gridColumns = 2;
    public Vector2 cellSize = new Vector2(200, 100);
    public Vector2 cellSpacing = new Vector2(15, 15);

    [Header("Icons")]
public Sprite goodIcon;      // Drag green checkmark here
public Sprite badIcon;       // Drag red warning/X here
public Sprite neutralIcon;   // Drag yellow circle/dash here
    
    // FIX: Don't initialize here - just declare
    public RectOffset gridPadding; // Remove = new RectOffset(20,20,10,10)
    
    private bool isMazeComplete = false;
    
    private void Awake()
    {
        // FIX: Initialize padding in Awake
        if (gridPadding == null)
            gridPadding = new RectOffset(20, 20, 10, 10);
        
        if (retryMazeButton != null)
            retryMazeButton.onClick.AddListener(OnRetryMaze);
            
        if (returnToCityButton != null)
            returnToCityButton.onClick.AddListener(OnReturnToCity);
            
        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseButton);
    }
    
    private void OnEnable()
    {
        StartCoroutine(DelayedRefresh());
    }
    
    private IEnumerator DelayedRefresh()
    {
        yield return null; // Wait one frame
        EnsureGridContainerExists();
        RefreshJourney();
    }
    
    private void EnsureGridContainerExists()
    {
        if (rewardGridContainer == null && contentParent != null)
        {
            Transform found = contentParent.Find("RewardGridContainer");
            if (found != null)
            {
                rewardGridContainer = found;
            }
            else
            {
                GameObject newContainer = new GameObject("RewardGridContainer");
                rewardGridContainer = newContainer.transform;
                rewardGridContainer.SetParent(contentParent, false);
                
                // Set sibling index to be last (so it appears after header)
                rewardGridContainer.SetAsLastSibling();
                
                SetupGridLayout();
            }
        }
    }
    
   public void RefreshJourney()
{
    if (TaxChoiceTracker.Instance == null) return;
    if (contentParent == null) return;
    
    EnsureGridContainerExists();
    if (rewardGridContainer == null) return;

    // FIRST: Find and store reference to ButtonsSection BEFORE clearing
    Transform buttonsSection = null;
    for (int i = 0; i < contentParent.childCount; i++)
    {
        Transform child = contentParent.GetChild(i);
        if (child.name == "ButtonsSection")
        {
            buttonsSection = child;
            Debug.Log($"✅ Found ButtonsSection at index {i}");
            break;
        }
    }

    // LIST OF THINGS TO PRESERVE (by name)
    HashSet<string> preserveNames = new HashSet<string>
    {
        "RewardGridContainer",
        "ButtonsSection"
    };
    
    // Clear everything EXCEPT preserved items
    List<GameObject> toDestroy = new List<GameObject>();
    for (int i = 0; i < contentParent.childCount; i++)
    {
        Transform child = contentParent.GetChild(i);
        
        // Skip if name is in preserve list
        if (preserveNames.Contains(child.name))
        {
            Debug.Log($"✅ Preserving: {child.name}");
            continue;
        }
        
        toDestroy.Add(child.gameObject);
    }
    
    foreach (GameObject child in toDestroy)
        if (child != null) Destroy(child);
    
    // Clear grid items
    for (int i = rewardGridContainer.childCount - 1; i >= 0; i--)
        Destroy(rewardGridContainer.GetChild(i).gameObject);
    
    // 1. Add choices
foreach (var choice in TaxChoiceTracker.Instance.choiceHistory)
{
    GameObject item = Instantiate(choiceItemPrefab, contentParent);
    
    // Set choice text (left side)
    TextMeshProUGUI choiceText = item.transform.Find("Choice Text")?.GetComponent<TextMeshProUGUI>();
    if (choiceText != null) 
        choiceText.text = choice.choiceText;
    
    // Set badge icon based on wisdom (Image only - no text)
    Image badgeImage = item.transform.Find("Badge")?.GetComponent<Image>();
    if (badgeImage != null)
    {
        // You'll assign these icons in the Inspector
        // goodIcon, badIcon, neutralIcon are public variables
        if (choice.wisdomDelta > 0)
            badgeImage.sprite = goodIcon;      // Green checkmark icon
        else if (choice.wisdomDelta < 0)
            badgeImage.sprite = badIcon;       // Red warning/X icon
        else
            badgeImage.sprite = neutralIcon;   // Yellow circle/dash icon
    }
    
    // Set explanation text (right side)
    TextMeshProUGUI explanationText = item.transform.Find("ExplanationText")?.GetComponent<TextMeshProUGUI>();
    if (explanationText != null)
    {
        // Use the explanation directly from the choice record
        explanationText.text = choice.explanation;
    }
}
    // 2. Add Rewards Header
    if (rewardsHeaderPrefab != null)
    {
        GameObject header = Instantiate(rewardsHeaderPrefab, contentParent);
        header.transform.SetAsLastSibling();
    }
    
    // 3. Setup Grid Layout
    SetupGridLayout();
    
    // 4. Add rewards with icons
if (rewardItemPrefab != null && TaxChoiceTracker.Instance != null)
{
    foreach (RewardData rewardData in TaxChoiceTracker.Instance.collectedRewards)  // ← Now using RewardData
    {
        GameObject rewardItem = Instantiate(rewardItemPrefab, rewardGridContainer);
        RewardItemUI rewardUI = rewardItem.GetComponent<RewardItemUI>();
        
        if (rewardUI != null)
        {
            // Use the rewardData directly - it already has name AND icon!
            rewardUI.Setup(rewardData.rewardName, rewardData.rewardIcon);
        }
    }
}

    
    // 5. Handle ButtonsSection
    if (buttonsSection == null || buttonsSection.gameObject == null)
    {
        Debug.Log("⚠️ ButtonsSection not found, creating new one...");
        buttonsSection = CreateButtonsSection();
    }
    else
    {
        // Make sure it's a child of contentParent
        buttonsSection.SetParent(contentParent, false);
    }
    
    // Ensure proper order
    rewardGridContainer.SetAsLastSibling();
    buttonsSection.SetAsLastSibling(); // Buttons at the very bottom
    
    // 6. Update button visibility
    isMazeComplete = MazeObjectiveManager.Instance != null && 
                     MazeObjectiveManager.Instance.mazeComplete;
    
    // Find buttons in ButtonsSection
    retryMazeButton = buttonsSection.Find("RetryMazeButton")?.GetComponent<Button>();
    returnToCityButton = buttonsSection.Find("ReturnToCityButton")?.GetComponent<Button>();
    closeButton = buttonsSection.Find("CloseButton")?.GetComponent<Button>();
    
    if (retryMazeButton != null)
    {
        retryMazeButton.gameObject.SetActive(true);
        Debug.Log($"Retry button active: {true}");
    }
    
    if (returnToCityButton != null)
    {
        returnToCityButton.gameObject.SetActive(true);
        Debug.Log("Return button active: true");
    }
    
    if (closeButton != null)
    {
        closeButton.gameObject.SetActive(true);
        Debug.Log("Close button active: true");
    }
    
    // Force layout rebuild
    LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent.GetComponent<RectTransform>());
}

private Transform CreateButtonsSection()
{
    GameObject buttonPanel = new GameObject("ButtonsSection");
    buttonPanel.transform.SetParent(contentParent, false);
    
    // Add HorizontalLayoutGroup
    HorizontalLayoutGroup layout = buttonPanel.AddComponent<HorizontalLayoutGroup>();
    layout.spacing = 20;
    layout.childAlignment = TextAnchor.MiddleCenter;
    layout.childControlWidth = true;
    layout.childControlHeight = true;
    layout.childForceExpandWidth = false;
    layout.childForceExpandHeight = false;
    layout.padding = new RectOffset(20, 20, 10, 10);
    
    // Add ContentSizeFitter
    ContentSizeFitter fitter = buttonPanel.AddComponent<ContentSizeFitter>();
    fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
    fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    
    // Add LayoutElement
    LayoutElement panelLayout = buttonPanel.AddComponent<LayoutElement>();
    panelLayout.minHeight = 80;
    panelLayout.preferredHeight = 80;
    
    // Create buttons
    CreateButton(buttonPanel.transform, "RetryMazeButton", "RETRY MAZE", new Color(0.2f, 0.6f, 0.2f));
    CreateButton(buttonPanel.transform, "ReturnToCityButton", "RETURN TO CITY", new Color(0.2f, 0.4f, 0.8f));
    CreateButton(buttonPanel.transform, "CloseButton", "CLOSE", new Color(0.5f, 0.5f, 0.5f));
    
    return buttonPanel.transform;
}

private void CreateButton(Transform parent, string name, string text, Color color)
{
    GameObject btnObj = new GameObject(name);
    btnObj.transform.SetParent(parent, false);
    
    // Button component
    Button btn = btnObj.AddComponent<Button>();
    
    // Image
    Image img = btnObj.AddComponent<Image>();
    img.color = color;
    
    // Text
    GameObject textObj = new GameObject("Text");
    textObj.transform.SetParent(btnObj.transform, false);
    TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
    tmp.text = text;
    tmp.color = Color.white;
    tmp.alignment = TextAlignmentOptions.Center;
    tmp.fontSize = 24;
    tmp.fontStyle = FontStyles.Bold;
    
    // Layout
    LayoutElement layout = btnObj.AddComponent<LayoutElement>();
    layout.minWidth = 180;
    layout.minHeight = 60;
    layout.preferredWidth = 180;
    layout.preferredHeight = 60;
    
    // Add listener based on button name
    if (name == "RetryMazeButton")
        btn.onClick.AddListener(OnRetryMaze);
    else if (name == "ReturnToCityButton")
        btn.onClick.AddListener(OnReturnToCity);
    else if (name == "CloseButton")
        btn.onClick.AddListener(OnCloseButton);
}
    void SetupGridLayout()
{
    if (rewardGridContainer == null) return;
    
    GridLayoutGroup grid = rewardGridContainer.GetComponent<GridLayoutGroup>();
    if (grid == null)
        grid = rewardGridContainer.gameObject.AddComponent<GridLayoutGroup>();
    
    grid.cellSize = cellSize;
    grid.spacing = cellSpacing;
    
    if (gridPadding != null)
        grid.padding = gridPadding;
    
    grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
    grid.constraintCount = gridColumns;
    grid.childAlignment = TextAnchor.MiddleCenter;
    
    // FIX: Add and configure ContentSizeFitter properly
    ContentSizeFitter fitter = rewardGridContainer.GetComponent<ContentSizeFitter>();
    if (fitter == null)
        fitter = rewardGridContainer.gameObject.AddComponent<ContentSizeFitter>();
    
    // Set BOTH horizontal and vertical
    fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize; // ← ADD THIS
    fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    
    // Optional: Add LayoutElement to control width
    LayoutElement layout = rewardGridContainer.GetComponent<LayoutElement>();
    if (layout == null)
        layout = rewardGridContainer.gameObject.AddComponent<LayoutElement>();
    
    // Let it stretch to parent width
    layout.flexibleWidth = 1; // ← This makes it take full width of parent
}
    
    // Button handlers
    public void OnReturnToCity() => mazeDialog?.ReturnToCity();
    public void OnRetryMaze() => mazeDialog?.RetryMaze();
    public void OnCloseButton() => gameObject.SetActive(false);
}