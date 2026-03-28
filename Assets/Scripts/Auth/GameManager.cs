using UnityEngine;
using TMPro;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    //[SerializeField]
    //private TMP_Text messageText;

    [SerializeField]
    private TMP_Text coinsDisplayText;
    [SerializeField]
    private TMP_Text levelDisplayText;

    [Header("Score UI Elements")]
    [SerializeField] private Slider scoreSlider;
    [SerializeField] private Image sliderFillImage;
    [SerializeField] private int maxScore = 250;

    [Header("Health UI Elements")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image healthSliderFillImage;
    [SerializeField] private int maxHealth = 250;
    //public static GameManager Instance { get; private set; }

    //private void Awake()
    //{
    //    if (Instance == null)
    //    {
    //        Instance = this;
    //    }
    //    else
    //    {
    //        Destroy(gameObject);
    //    }
    //}

    void Start()
    {
        //ShowMessage();
        InitializeScoreSlider();
        InitializeHealthSlider();
        GetCoins();
        GetScore();
        GetHealth();
        GetLevel();
    }

    //private void ShowMessage()
    //{
    //    messageText.text = string.Format("User : {0}", References.userName);

    //}

    private void InitializeScoreSlider()
    {
        if (scoreSlider != null)
        {
            scoreSlider.minValue = 0;
            scoreSlider.maxValue = maxScore;
            scoreSlider.value = 0;

            // Update text if available
            //UpdateScoreUI(0);
        }
    }

    private void InitializeHealthSlider()
    {
        if (healthSlider != null)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = maxHealth;
            healthSlider.value = 0;

            // Update text if available
            //UpdateScoreUI(0);
        }
    }

    public void CLearGame()
    {
        SaveController.Instance.ClearSaveGame();
    }

    public void SaveGame()
    {
        SaveController.Instance.SaveGame();
    }

    public void GoToMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("opening");
    }

    // Helper method to get Firestore document
    public static DocumentReference GetUserDocument()
    {
        if (References.Firestore != null && References.User != null)
        {
            return References.Firestore.Collection("users").Document(References.User.UserId);
        }
        return null;
    }

    //use this to fetch one value for one key
    public void GetCoins() // Button calls this
    {
        StartCoroutine(GetCoinsCoroutine());
    }

    private IEnumerator GetCoinsCoroutine()
    {
        var userDoc = GetUserDocument();
        if (userDoc != null)
        {
            var loadTask = userDoc.GetSnapshotAsync();

            yield return new WaitUntil(() => loadTask.IsCompleted);

            if (loadTask.Exception != null)
            {
                Debug.LogWarning($"Failed to load data: {loadTask.Exception}");
            }
            else
            {
                DocumentSnapshot snapshot = loadTask.Result;

                if (snapshot.Exists)
                {
                    // Safely get data with default values
                    if (snapshot.ContainsField("coins"))
                    {
                        int coins = snapshot.GetValue<int>("coins");
                        Debug.Log($"coins: {coins}");
                        coinsDisplayText.text = coins.ToString();
                        References.userCoins = coins;
                    }
                    else
                    {
                        Debug.Log("No coins field found");
                    }
                }
            }
        }
    }

    public void updateCoinsEnumCall(int coins)
    {
        StartCoroutine(UpdateCoins(coins));
        GetCoins();
        //StartCoroutine(UpdateKills(int.Parse(0)));
        //StartCoroutine(UpdateDeaths(int.Parse(20)));
    }

    private IEnumerator UpdateCoins(int _coins)
    {
        if (_coins < 0)
        {
            Debug.LogWarning($"Invalid coin amount: {_coins}. Coins cannot be negative.");
            yield break;
        }

        var userDoc = GetUserDocument();
        var new_coins = _coins + References.userCoins;
        Dictionary<string, object> updates = new Dictionary<string, object>
        {
            { "coins", new_coins },
            { "lastUpdated", FieldValue.ServerTimestamp } // Track when updated
        };

        // Use UpdateAsync to only modify specific fields
        var updateTask = userDoc.UpdateAsync(updates);

        // Add a timeout to prevent hanging
        float timeout = 10f;
        float startTime = Time.time;

        while (!updateTask.IsCompleted && Time.time - startTime < timeout)
        {
            yield return null;
        }

        if (!updateTask.IsCompleted)
        {
            Debug.LogError($"Update timeout after {timeout} seconds");
            yield break;
        }

        if (updateTask.Exception != null)
        {
            // Check if document exists, if not create it
            if (updateTask.Exception.Message.Contains("No document to update"))
            {
                Debug.LogWarning("Document doesn't exist.");
                //yield return CreateUserDocument(user.UserId, user.DisplayName, user.Email, _coins);
            }
            else
            {
                Debug.LogError($"Failed to update coins: {updateTask.Exception.Message}");

                // Log Firebase-specific errors
                Firebase.FirebaseException firestoreException =
                    updateTask.Exception.GetBaseException() as Firebase.FirebaseException;
                if (firestoreException != null)
                {
                    Debug.LogError($"Firestore error code: {firestoreException.ErrorCode}");
                }
            }
        }
        else
        {
            Debug.Log($"Successfully updated coins to {_coins} in Firestore");

            // Optionally update local reference immediately
            References.userCoins = _coins;
            coinsDisplayText.text = References.userCoins.ToString();
            GetHealth();
        }
        //}
        //catch (System.Exception e)
        //{
        //    Debug.LogError($"Unexpected error in UpdateXp: {e.Message}\n{e.StackTrace}");
        //}
    }


    public void GetScore()
    {
        StartCoroutine(GetScoreCoroutine());
    }

    private IEnumerator GetScoreCoroutine()
    {
        var userDoc = GetUserDocument();
        if (userDoc != null)
        {
            var loadTask = userDoc.GetSnapshotAsync();

            yield return new WaitUntil(() => loadTask.IsCompleted);

            if (loadTask.Exception != null)
            {
                Debug.LogWarning($"Failed to load data: {loadTask.Exception}");
            }
            else
            {
                DocumentSnapshot snapshot = loadTask.Result;

                if (snapshot.Exists)
                {
                    // Safely get data with default values
                    if (snapshot.ContainsField("knowledge "))
                    {
                        int score = snapshot.GetValue<int>("knowledge ");
                        Debug.Log($"knowledge: {score}");
                        UpdateScoreUI(score);
                        References.userScore = score;
                    }
                    else
                    {
                        UpdateScoreUI(0);
                        Debug.Log("No score field found");
                    }
                }
            }
        }
    }

    public void GetHealth()
    {
        StartCoroutine(GetHealthCoroutine());
    }

    private IEnumerator GetHealthCoroutine()
    {
        var userDoc = GetUserDocument();
        if (userDoc != null)
        {
            var loadTask = userDoc.GetSnapshotAsync();

            yield return new WaitUntil(() => loadTask.IsCompleted);

            if (loadTask.Exception != null)
            {
                Debug.LogWarning($"Failed to load data: {loadTask.Exception}");
            }
            else
            {
                DocumentSnapshot snapshot = loadTask.Result;

                if (snapshot.Exists)
                {
                    // Safely get data with default values
                    if (snapshot.ContainsField("health"))
                    {
                        int health = snapshot.GetValue<int>("health");
                        Debug.Log($"health: {health}");
                        UpdateHealthUI(health);
                        References.userHealth = health;
                    }
                    else
                    {
                        UpdateHealthUI(0);
                        Debug.Log("No health field found");
                    }
                }
            }
        }
    }

    public void GetLevel()
    {
        StartCoroutine(GetLevelCoroutine());
    }

    private IEnumerator GetLevelCoroutine()
    {
        var userDoc = GetUserDocument();
        if (userDoc != null)
        {
            var loadTask = userDoc.GetSnapshotAsync();

            yield return new WaitUntil(() => loadTask.IsCompleted);

            if (loadTask.Exception != null)
            {
                Debug.LogWarning($"Failed to load data: {loadTask.Exception}");
            }
            else
            {
                DocumentSnapshot snapshot = loadTask.Result;

                if (snapshot.Exists)
                {
                    // Safely get data with default values
                    if (snapshot.ContainsField("level"))
                    {
                        int level = snapshot.GetValue<int>("level");
                        Debug.Log($"level: {level}");
                        UpdateLevelUI(level);
                        References.userLevel = level;
                    }
                    else
                    {
                        UpdateLevelUI(0);
                        Debug.Log("No score field found");
                    }
                }
            }
        }
    }

    public void updateLevelEnumCall(int increase)
    {
        StartCoroutine(UpdateLevel(increase));
        
    }

    private IEnumerator UpdateLevel(int _increase)
    {
        if (_increase < 0)
        {
            Debug.LogWarning($"Invalid coin amount: {_increase}. Coins cannot be negative.");
            yield break;
        }

        var userDoc = GetUserDocument();
        var new_level = _increase + References.userLevel;
        Dictionary<string, object> updates = new Dictionary<string, object>
        {
            { "level", new_level },
            { "lastUpdated", FieldValue.ServerTimestamp } // Track when updated
        };

        // Use UpdateAsync to only modify specific fields
        var updateTask = userDoc.UpdateAsync(updates);

        // Add a timeout to prevent hanging
        float timeout = 10f;
        float startTime = Time.time;

        while (!updateTask.IsCompleted && Time.time - startTime < timeout)
        {
            yield return null;
        }

        if (!updateTask.IsCompleted)
        {
            Debug.LogError($"Update timeout after {timeout} seconds");
            yield break;
        }

        if (updateTask.Exception != null)
        {
            // Check if document exists, if not create it
            if (updateTask.Exception.Message.Contains("No document to update"))
            {
                Debug.LogWarning("Document doesn't exist.");
                //yield return CreateUserDocument(user.UserId, user.DisplayName, user.Email, _coins);
            }
            else
            {
                Debug.LogError($"Failed to update coins: {updateTask.Exception.Message}");

                // Log Firebase-specific errors
                Firebase.FirebaseException firestoreException =
                    updateTask.Exception.GetBaseException() as Firebase.FirebaseException;
                if (firestoreException != null)
                {
                    Debug.LogError($"Firestore error code: {firestoreException.ErrorCode}");
                }
            }
        }
        else
        {
            Debug.Log($"Successfully updated coins to {_increase} in Firestore");

            // Optionally update local reference immediately
            References.userLevel = _increase;
            levelDisplayText.text = References.userLevel.ToString();
            GetLevel();
        }
        //}
        //catch (System.Exception e)
        //{
        //    Debug.LogError($"Unexpected error in UpdateXp: {e.Message}\n{e.StackTrace}");
        //}
    }

    public void updateScoreEnumCall(int score)
    {
        StartCoroutine(UpdateScore(score));
        
    }

    private IEnumerator UpdateScore(int _score)
    {
        if (_score < 0)
        {
            Debug.LogWarning($"Invalid score amount: {_score}. Score cannot be negative.");
            yield break;
        }

        var userDoc = GetUserDocument();
        var new_score = _score + References.userScore;
        Dictionary<string, object> updates = new Dictionary<string, object>
        {
            { "knowledge ", new_score },
            { "lastUpdated", FieldValue.ServerTimestamp } // Track when updated
        };

        // Use UpdateAsync to only modify specific fields
        var updateTask = userDoc.UpdateAsync(updates);

        // Add a timeout to prevent hanging
        float timeout = 10f;
        float startTime = Time.time;

        while (!updateTask.IsCompleted && Time.time - startTime < timeout)
        {
            yield return null;
        }

        if (!updateTask.IsCompleted)
        {
            Debug.LogError($"Update timeout after {timeout} seconds");
            yield break;
        }

        if (updateTask.Exception != null)
        {
            // Check if document exists, if not create it
            if (updateTask.Exception.Message.Contains("No document to update"))
            {
                Debug.LogWarning("Document doesn't exist.");
                //yield return CreateUserDocument(user.UserId, user.DisplayName, user.Email, _coins);
            }
            else
            {
                Debug.LogError($"Failed to update score: {updateTask.Exception.Message}");

                // Log Firebase-specific errors
                Firebase.FirebaseException firestoreException =
                    updateTask.Exception.GetBaseException() as Firebase.FirebaseException;
                if (firestoreException != null)
                {
                    Debug.LogError($"Firestore error code: {firestoreException.ErrorCode}");
                }
            }
        }
        else
        {
            Debug.Log($"Successfully updated knowledge to {_score} in Firestore");
            // Optionally update local reference immediately
            References.userScore= new_score;
            UpdateScoreUI(References.userScore);
            GetScore();
        }
        //}
        //catch (System.Exception e)
        //{
        //    Debug.LogError($"Unexpected error in UpdateXp: {e.Message}\n{e.StackTrace}");
        //}
    }

    // Change max score value dynamically (e.g., when player levels up)
    public void SetMaxScore(int newMaxScore)
    {
        maxScore = newMaxScore;
        if (scoreSlider != null)
        {
            scoreSlider.maxValue = maxScore;
            UpdateScoreUI(References.userScore);
        }
    }

    // Check if score has reached max
    public bool IsScoreMaxed()
    {
        return References.userScore >= maxScore;
    }

    // Update all score-related UI elements
    public void UpdateScoreUI(int score)
    {
        // Update slider if available
        if (scoreSlider != null)
        {
            scoreSlider.value = Mathf.Clamp(score, 0, maxScore);

            // Update slider color based on progress
            //UpdateSliderColor(score);
        }

        //// Update score text if available
        //if (scoreText != null)
        //{
        //    scoreText.text = $"{score}/{maxScore}";
        //}

        // Update score value text if available
        //if (scoreValueText != null)
        //{
        //    scoreValueText.text = score.ToString();
        //}
    }

    public void UpdateHealthUI(int score)
    {
        // Update slider if available
        if (healthSlider != null)
        {
            healthSlider.value = Mathf.Clamp(score, 0, maxScore);

            // Update slider color based on progress
            //UpdateSliderColor(score);
        }
    }

    public void UpdateCoinsUI(int coins)
    {
        // Update slider if available
        if (coinsDisplayText != null)
        {
            coinsDisplayText.text = coins.ToString();

            // Update slider color based on progress
            //UpdateSliderColor(score);
        }
    }

    public void UpdateLevelUI(int coins)
    {
        // Update slider if available
        if (levelDisplayText != null)
        {
            levelDisplayText.text = coins.ToString();

            // Update slider color based on progress
            //UpdateSliderColor(score);
        }
    }
}