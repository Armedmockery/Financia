using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore; 
using System.Linq;
using System.Threading.Tasks;
public class FirebaseAuthManager : MonoBehaviour
{
    public static FirebaseAuthManager Instance { get; private set; }
    public static bool IsFirstTimePlaying { get; private set; } = false;
    // Firebase variable
    [Header("Firebase (Runtime)")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser user;
    public FirebaseFirestore db;

    // Login Variables
    [Header("Login")]
    [SerializeField]
    public TMP_InputField emailLoginField;
    [SerializeField]
    public TMP_InputField passwordLoginField;

    // Registration Variables
    [Header("Registration")]
    [SerializeField]
    public TMP_InputField nameRegisterField;
    [SerializeField]
    public TMP_InputField emailRegisterField;
    [SerializeField]
    public TMP_InputField passwordRegisterField;
    [SerializeField]
    public TMP_InputField confirmPasswordRegisterField;

    [Header("Delete Account Re-Auth")]
    [SerializeField] 
    public GameObject deleteAccountPanel;
    [SerializeField] 
    public TMP_InputField emailDeleteField;
    [SerializeField] 
    public TMP_InputField passwordDeleteField;
    [SerializeField] private SceneLoader sceneLoader;
    private void Start()
    {
        StartCoroutine(CheckAndFixDependenciesAsync());
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

/*    private void Awake()
    {
        // Check that all of the necessary dependencies for firebase are present on the system
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;

            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all firebase dependencies: " + dependencyStatus);
            }
        });
    }*/

    private IEnumerator CheckAndFixDependenciesAsync()
    {
        var dependencyTask = FirebaseApp.CheckAndFixDependenciesAsync();
        yield return new WaitUntil(() =>  dependencyTask.IsCompleted);
        dependencyStatus = dependencyTask.Result;

        if (dependencyStatus == DependencyStatus.Available)
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            if (app == null)
            {
                app = FirebaseApp.Create();
            }
            InitializeFirebase();
            yield return new WaitForEndOfFrame();
            StartCoroutine(CheckForAutoLogin());
        }
        else
        {
            Debug.LogError("Could not resolve all firebase dependencies: " + dependencyStatus);
        }
    }
    
    private IEnumerator CheckForAutoLogin()
    {
        if(user != null)
        {
            var reloadUserTask = user.ReloadAsync();
            yield return new WaitUntil(() =>  reloadUserTask.IsCompleted);
            AutoLogin();
            StartCoroutine(LoadUserData());
            yield return new WaitForSeconds(2);
        }
        else
        {
        UIManager.Instance.OpenLoginPanel();
        }
    }
    
    private void AutoLogin()
    {
        if (user != null)
        {
                References.userName = user.DisplayName;
                References.userEmail = user.Email;
                UIManager.Instance.OpenGamePanel();
                UIManager.Instance.RefreshUserData();
                Debug.Log("User logged in automatically : " + References.userName);

        }
        else
        {
            UIManager.Instance.OpenLoginPanel();
        }
}    

    void InitializeFirebase()
    {
        //Set the default instance object
        auth = FirebaseAuth.DefaultInstance;

        // Wait a frame before initializing Firestore
        StartCoroutine(InitializeFirestoreDelayed());

        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    private IEnumerator InitializeFirestoreDelayed()
    {
        yield return null; // Wait one frame

        try
        {
            Debug.Log("Initializing Firestore...");
            db = FirebaseFirestore.DefaultInstance;
            Debug.Log("Firestore initialized successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Firestore initialization failed: " + e.Message);
        }
    }

    // Track state changes of the auth object.
    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;

            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
                UIManager.Instance.OpenRegistrationPanel();
                ClearInputFields();
            }

            user = auth.CurrentUser;

            if (signedIn)
            {
                Debug.Log("Signed in " + user.UserId);
            }
        }
    }

    private void ClearInputFields()
    {
        emailLoginField.text = "";
        emailRegisterField.text = "";
        passwordLoginField.text = "";
        passwordRegisterField.text = "";
        confirmPasswordRegisterField.text = "";
        nameRegisterField.text = "";
    }

    public void Logout()
    {
        if(auth != null && user != null)
        {
            auth.SignOut();
        }
    }

    public void ConfirmDeleteAccount()
    {
        StartCoroutine(DeleteAccountAsync());
    }

    // Delete user without user data deletion for now.
    private IEnumerator DeleteAccountAsync()
    {
        if (user == null)
        {
            Debug.LogError("No logged-in user.");
            yield break;
        }

        string email = emailDeleteField.text;
        string password = passwordDeleteField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            PopupsScript.Instance.ShowItemPickup("Email or password is empty.");
            Debug.LogError("Email or password is empty.");
            yield break;
        }

        Credential credential = EmailAuthProvider.GetCredential(email, password);

        var reauthTask = user.ReauthenticateAsync(credential);
        yield return new WaitUntil(() => reauthTask.IsCompleted);

        if (reauthTask.Exception != null)
        {
            Debug.LogError("Re-authentication failed: " + reauthTask.Exception);
            yield break;
        }

        var deleteTask = user.DeleteAsync();
        yield return new WaitUntil(() => deleteTask.IsCompleted);

        if (deleteTask.Exception != null)
        {
            PopupsScript.Instance.ShowItemPickup("Account deletion failed!");
            Debug.LogError("Account deletion failed: " + deleteTask.Exception);
            yield break;
        }

        Debug.Log("Account deleted successfully.");

        auth.SignOut();
        user = null;

        UIManager.Instance.CloseDeleteAccountPanel();
        ClearInputFields();
        UIManager.Instance.OpenRegistrationPanel();
    }

    public void Login()
    {
        StartCoroutine(LoginAsync(emailLoginField.text, passwordLoginField.text));
    }

    private IEnumerator LoginAsync(string email, string password)
    {
        var loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);

        yield return new WaitUntil(() => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            Debug.LogError(loginTask.Exception);

            FirebaseException firebaseException = loginTask.Exception.GetBaseException() as FirebaseException;
            AuthError authError = (AuthError)firebaseException.ErrorCode;


            string failedMessage = "Login Failed! Because ";

            switch (authError)
            {
                case AuthError.InvalidEmail:
                    failedMessage += "Email is invalid";
                    break;
                case AuthError.WrongPassword:
                    failedMessage += "Wrong Password";
                    break;
                case AuthError.MissingEmail:
                    failedMessage += "Email is missing";
                    break;
                case AuthError.MissingPassword:
                    failedMessage += "Password is missing";
                    break;
                default:
                    failedMessage = "Login Failed";
                    break;
            }

            Debug.Log(failedMessage);
            PopupsScript.Instance.ShowItemPickup(failedMessage);
        }
        else
        {
            user = loginTask.Result.User;

            Debug.LogFormat("{0} You Are Successfully Logged In", user.DisplayName);
            if (user.IsEmailVerified)
            {
                References.userName = user.DisplayName;
                References.userEmail = user.Email;
                UIManager.Instance.OpenGamePanel();
                UIManager.Instance.RefreshUserData();

                //User is now logged in
                //Now get the result
                StartCoroutine(LoadUserData());
                yield return new WaitForSeconds(2);
            }
            else
            {
                SendEmailForVerification();
            }
            
        }
    }

    public void Register()
    {
        StartCoroutine(RegisterAsync(nameRegisterField.text, emailRegisterField.text, passwordRegisterField.text, confirmPasswordRegisterField.text));
    }

    private IEnumerator RegisterAsync(string name, string email, string password, string confirmPassword)
    {
        if (name == "")
        {
            PopupsScript.Instance.ShowItemPickup("User Name is empty");
            Debug.LogError("User Name is empty");
        }
        else if (email == "")
        {
            PopupsScript.Instance.ShowItemPickup("Email field is empty");
            Debug.LogError("email field is empty");
        }
        else if (passwordRegisterField.text != confirmPasswordRegisterField.text)
        {
            PopupsScript.Instance.ShowItemPickup("Password does not match");
            Debug.LogError("Password does not match");
        }
        else
        {
            var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);

            yield return new WaitUntil(() => registerTask.IsCompleted);

            if (registerTask.Exception != null)
            {
                Debug.LogError(registerTask.Exception);

                FirebaseException firebaseException = registerTask.Exception.GetBaseException() as FirebaseException;
                AuthError authError = (AuthError)firebaseException.ErrorCode;

                string failedMessage = "Registration Failed! Becuase ";
                switch (authError)
                {
                    case AuthError.InvalidEmail:
                        failedMessage += "Email is invalid";
                        break;
                    case AuthError.WrongPassword:
                        failedMessage += "Wrong Password";
                        break;
                    case AuthError.MissingEmail:
                        failedMessage += "Email is missing";
                        break;
                    case AuthError.MissingPassword:
                        failedMessage += "Password is missing";
                        break;
                    default:
                        failedMessage = "Registration Failed";
                        break;
                }
                PopupsScript.Instance.ShowItemPickup(failedMessage);
                Debug.Log(failedMessage);
            }
            else
            {
                // Get The User After Registration Success
                user = registerTask.Result.User;

                UserProfile userProfile = new UserProfile { DisplayName = name };

                var updateProfileTask = user.UpdateUserProfileAsync(userProfile);

                yield return new WaitUntil(() => updateProfileTask.IsCompleted);

                if (updateProfileTask.Exception != null)
                {
                    // Delete the user if user update failed
                    user.DeleteAsync();

                    Debug.LogError(updateProfileTask.Exception);

                    FirebaseException firebaseException = updateProfileTask.Exception.GetBaseException() as FirebaseException;
                    AuthError authError = (AuthError)firebaseException.ErrorCode;


                    string failedMessage = "Profile update Failed! Becuase ";
                    switch (authError)
                    {
                        case AuthError.InvalidEmail:
                            failedMessage += "Email is invalid";
                            break;
                        case AuthError.WrongPassword:
                            failedMessage += "Wrong Password";
                            break;
                        case AuthError.MissingEmail:
                            failedMessage += "Email is missing";
                            break;
                        case AuthError.MissingPassword:
                            failedMessage += "Password is missing";
                            break;
                        default:
                            failedMessage = "Profile update Failed";
                            break;
                    }
                    PopupsScript.Instance.ShowItemPickup(failedMessage);
                    Debug.Log(failedMessage);
                }
                else
                { 
                    Debug.Log("Registration Sucessful Welcome " + user.DisplayName);
                    if (user.IsEmailVerified)
                    {
                        UIManager.Instance.OpenLoginPanel();
                    }
                    else
                    {
                        SendEmailForVerification();
                    }
                }
            }
        }
    }

    public void SendEmailForVerification()
    {
        StartCoroutine(SendEmailForVerificationAsync());
        Debug.Log("Verification email sent. Please verify before login.");
        //UIManager.Instance.ShowVerificationResponse(true, user.Email, null);
        //UIManager.Instance.OpenLoginPanel();
    }

    private IEnumerator SendEmailForVerificationAsync()
    {
        if(user != null)
        {
            var sendEmailTask = user.SendEmailVerificationAsync();
            yield return new WaitUntil(() =>  sendEmailTask.IsCompleted);
            if (sendEmailTask.Exception != null) { 
                FirebaseException firebaseException = sendEmailTask.Exception.GetBaseException() as FirebaseException;
                AuthError error = (AuthError)firebaseException.ErrorCode;
                string errorMessage = "Unknown error : Please try again later";
                switch (error) {
                    case AuthError.Cancelled:
                        errorMessage = "Email Verification was cancelled";
                        break;
                    case AuthError.TooManyRequests:
                        errorMessage = "Too many reuqests.";
                        break;
                    case AuthError.InvalidRecipientEmail:
                        errorMessage = "Invalid email entered!";
                        break;
                }
                UIManager.Instance.ShowVerificationResponse(false, user.Email, errorMessage);
                PopupsScript.Instance.ShowItemPickup(errorMessage);
            }
            else
            {
                Debug.Log("Email has been succesfully sent.");
                UIManager.Instance.ShowVerificationResponse(true, user.Email, null);
                Debug.Log("Verification task completed. Faulted: " + sendEmailTask.IsFaulted);
            }
        }
    }

    public void OpenGameScene()
    {
        sceneLoader.LoadGameScene("Test Prithvi");
    }

    /**************************************DATABASE CODES***********************************/
    //sample data update
    public void updateCoinsEnumCall(int coins)
    {
        StartCoroutine(UpdateCoins(coins));
        //StartCoroutine(UpdateKills(int.Parse(0)));
        //StartCoroutine(UpdateDeaths(int.Parse(20)));
    }

    //sample fetch data
    //public void ScoreboardButton()
    //{
    //    StartCoroutine(LoadScoreboardData());
    //}

    private IEnumerator UpdateCoins(int _coins)
    {
        // Validate inputs and state
        if (db == null)
        {
            Debug.LogError("Firestore database not initialized!");
            yield break;
        }

        if (user == null)
        {
            PopupsScript.Instance.ShowItemPickup("No user logged in!");
            Debug.LogError("No user logged in!");
            yield break;
        }

        if (_coins < 0)
        {
            Debug.LogWarning($"Invalid coin amount: {_coins}. Coins cannot be negative.");
            yield break;
        }

        DocumentReference userDoc = db.Collection("users").Document(user.UserId);
        Debug.Log($"Attempting to update coins to {_coins} for user: {user.UserId}");

        //try
        //{
            // Create a dictionary with the field to update
            Dictionary<string, object> updates = new Dictionary<string, object>
        {
            { "coins", _coins },
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
            }
        //}
        //catch (System.Exception e)
        //{
        //    Debug.LogError($"Unexpected error in UpdateXp: {e.Message}\n{e.StackTrace}");
        //}
    }


    private IEnumerator LoadUserData()
    {
        // Add safety checks
        if (db == null)
        {
            Debug.LogError("Firestore not initialized!");
            yield break;
        }

        if (user == null)
        {
            Debug.LogError("User not logged in!");
            yield break;
        }

        DocumentReference userDoc = db.Collection("users").Document(user.UserId);
        Debug.Log($"Attempting to load user data for: {user.UserId}");

        var loadTask = userDoc.GetSnapshotAsync();

        yield return new WaitUntil(() => loadTask.IsCompleted);

        if (loadTask.Exception != null)
        {
            Debug.LogWarning($"Failed to load data: {loadTask.Exception}");
            // Don't crash - create document if it doesn't exist
            yield return CreateUserDocumentIfNeeded();
        }
        else
        {
            DocumentSnapshot snapshot = loadTask.Result;

            if (snapshot.Exists)
            {
                //if (snapshot.ContainsField("isFirstTimePlaying"))
                //{
                //    IsFirstTimePlaying = snapshot.GetValue<bool>("isFirstTimePlaying");
                //    Debug.Log($"IsFirstTimePlaying: {IsFirstTimePlaying}");
                //}
                // Safely get data with default values
                if (snapshot.ContainsField("coins"))
                {
                    int coins = snapshot.GetValue<int>("coins");
                    Debug.Log($"Coins: {coins}");
                    // Store in your game manager
                    References.userCoins = coins;
                }
                else
                {
                    Debug.Log("No coins field found");
                    //yield return UpdateUserData(0); // Create coins field with 0
                }
            }
            else
            {
                Debug.LogWarning("User document does not exist. Creating...");
                yield return CreateUserDocumentIfNeeded();
                snapshot = loadTask.Result;

                if (snapshot.Exists)
                {
                    // Safely get data with default values
                    if (snapshot.ContainsField("coins"))
                    {
                        int coins = snapshot.GetValue<int>("coins");
                        Debug.Log($"Coins: {coins}");
                        // Store in your game manager
                        References.userCoins = coins;
                    }
                    else
                    {
                        Debug.Log("No coins field found");
                        //yield return UpdateUserData(0); // Create coins field with 0
                    }
                }
            }
        }
    }

    //public IEnumerator MarkFirstPlayComplete()
    //{
    //    if (db == null || user == null) yield break;

    //    DocumentReference userDoc = db.Collection("users").Document(user.UserId);
    //    var updateTask = userDoc.UpdateAsync("isFirstTimePlaying", false);
    //    yield return new WaitUntil(() => updateTask.IsCompleted);

    //    if (updateTask.Exception != null)
    //        Debug.LogError("Failed to update isFirstTimePlaying: " + updateTask.Exception);
    //    else
    //    {
    //        IsFirstTimePlaying = false;
    //        Debug.Log("First play marked as complete.");
    //    }
    //}

    // Add this method to your FirebaseAuthManager class
    private IEnumerator CreateUserDocumentIfNeeded()
    {
        if (db == null || user == null)
        {
            Debug.LogError("Cannot create document - Firestore or User is null");
            yield break;
        }

        DocumentReference userDoc = db.Collection("users").Document(user.UserId);

        // Create a new user document with default values
        Dictionary<string, object> userData = new Dictionary<string, object>
    {
        { "coins", 0 },
        { "knowledge ", 0 },
        { "health", 0 },
        { "name", user.DisplayName ?? "" },
        { "email", user.Email ?? "" },
        { "current_quest", null },
        { "level", 0 },
        { "quest_status", "NOT_STARTED" },
        { "achievements", new List<object>() },
        { "isFirstTimePlaying", true },        // ← ADD THIS LINE
        { "createdAt", FieldValue.ServerTimestamp },
        { "lastUpdated", FieldValue.ServerTimestamp }
    };

        var setTask = userDoc.SetAsync(userData);
        yield return new WaitUntil(() => setTask.IsCompleted);

        if (setTask.Exception != null)
        {
            Debug.LogError($"Failed to create user document: {setTask.Exception}");
            yield break;
        }

        Debug.Log("Successfully created new user document with default values");

        // Create initial inventory document (you can customize this)
        Dictionary<string, object> inventoryData = new Dictionary<string, object>
    {
        { "item", "dagger" },
        { "quantity", 1 },
        { "createdAt", FieldValue.ServerTimestamp }
    };

        var inventoryTask = userDoc.Collection("inventory").Document("dagger").SetAsync(inventoryData);
        yield return new WaitUntil(() => inventoryTask.IsCompleted);

        if (inventoryTask.Exception != null)
        {
            Debug.LogWarning($"Failed to create initial inventory: {inventoryTask.Exception}");
        }
        else
        {
            Debug.Log("Created initial inventory document");
        }

        // Create initial quests document (you can customize this)
        Dictionary<string, object> questsData = new Dictionary<string, object>
    {
        { "quest_001", "quest_001" },
        { "coins", 100 },
        { "knowledge ", 100 },
        { "progress", 0 },
        { "rewards", new List<object>() },
        { "status", "NOT_STARTED" },
        { "createdAt", FieldValue.ServerTimestamp }
    };

        var questsTask = userDoc.Collection("quests").Document("quest_001").SetAsync(questsData);
        yield return new WaitUntil(() => questsTask.IsCompleted);

        if (questsTask.Exception != null)
        {
            Debug.LogWarning($"Failed to create initial quests: {questsTask.Exception}");
        }
        else
        {
            Debug.Log("Created initial quests document");
        }
        // Set local references
        References.userCoins = 0;
        References.userScore = 0;
        References.userHealth = 0;
        References.userName = user.DisplayName ?? "";
        References.userEmail = user.Email ?? "";
    }

    //private IEnumerator LoadScoreboardData()
    //{
    //    Query query = db.Collection("users")
    //                    .OrderByDescending("coins");

    //    var queryTask = query.GetSnapshotAsync();

    //    yield return new WaitUntil(() => queryTask.IsCompleted);

    //    if (queryTask.Exception != null)
    //    {
    //        Debug.LogWarning($"Failed to load scoreboard: {queryTask.Exception}");
    //    }
    //    else
    //    {
    //        QuerySnapshot snapshot = queryTask.Result;

    //        foreach (DocumentSnapshot doc in snapshot.Documents)
    //        {
    //            string username = doc.GetValue<string>("name");
    //            int coins = doc.GetValue<int>("coins");

    //            Debug.Log($"User: {username} | Coins: {coins}");
    //        }
    //    }
    //}

}
