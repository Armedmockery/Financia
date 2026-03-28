using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    [SerializeField]
    private TMP_Text emailVerificationText;

    [SerializeField]
    private TMP_Text nameDisplay;

    [SerializeField]
    private TMP_Text emailDisplay;

    [SerializeField]
    private GameObject loginPanel;

    [SerializeField]
    private GameObject registrationPanel;

    [SerializeField]
    private GameObject profilePanel;

    [SerializeField]
    private GameObject gamePanel;

    [SerializeField]
    private GameObject EmailVerificationPanel;

    [SerializeField]
    private GameObject deleteAccountPanel;

    [SerializeField]
    private GameObject exitConfirmationPanel;

    [Header("Password Fields")]
    [SerializeField] private TMP_InputField loginPasswordInput;
    [SerializeField] private TMP_InputField registerPasswordInput;
    [SerializeField] private TMP_InputField registerConfirmPasswordInput;
    [SerializeField] private TMP_InputField deleteConfirmPasswordInput;

    [Header("Password Toggle Buttons")]
    [SerializeField] private Button loginPasswordToggleBtn;
    [SerializeField] private Button registerPasswordToggleBtn;
    [SerializeField] private Button registerConfirmPasswordToggleBtn;
    [SerializeField] private Button deleteConfirmPasswordToggleBtn;

    [Header("Toggle Icons")]
    [SerializeField] private Sprite visibleIcon;        // Eye open icon
    [SerializeField] private Sprite hiddenIcon;         // Eye closed icon

    private bool isLoginPasswordVisible = false;
    private bool isRegisterPasswordVisible = false;
    private bool isConfirmPasswordVisible = false;
    private bool isDeleteConfirmPasswordVisible = false;

    private void Awake()
    {
        CreateInstance();
    }

    private void CreateInstance()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        InitializePasswordToggles();
    }

    public void OpenLoginPanel()
    {
        loginPanel.SetActive(true);
        registrationPanel.SetActive(false);
        EmailVerificationPanel.SetActive(false);
        profilePanel.SetActive(false);
        gamePanel.SetActive(false);
        deleteAccountPanel.SetActive(false);
        exitConfirmationPanel.SetActive(false);
        // Reset to asterisks when opening login panel
        ResetPasswordField(loginPasswordInput, loginPasswordToggleBtn, ref isLoginPasswordVisible);
    }

    public void OpenGamePanel()
    {
        loginPanel.SetActive(false);
        registrationPanel.SetActive(false);
        EmailVerificationPanel.SetActive(false);
        profilePanel.SetActive(false);
        gamePanel.SetActive(true);
        deleteAccountPanel.SetActive(false);
        exitConfirmationPanel.SetActive(false);
    }

    public void OpenRegistrationPanel()
    {
        registrationPanel.SetActive(true);
        loginPanel.SetActive(false);
        EmailVerificationPanel.SetActive(false);
        profilePanel.SetActive(false);
        gamePanel.SetActive(false);
        deleteAccountPanel.SetActive(false);
        exitConfirmationPanel.SetActive(false);
        // Reset to asterisks when opening registration panel
        ResetPasswordField(registerPasswordInput, registerPasswordToggleBtn, ref isRegisterPasswordVisible);
        ResetPasswordField(registerConfirmPasswordInput, registerConfirmPasswordToggleBtn, ref isConfirmPasswordVisible);
    
    }

    public void OpenProfilePanel()
    {
        profilePanel.SetActive(true);
        registrationPanel.SetActive(false);
        loginPanel.SetActive(false);
        EmailVerificationPanel.SetActive(false);
        gamePanel.SetActive(false);
        deleteAccountPanel.SetActive(false);
        exitConfirmationPanel.SetActive(false);
    }

    public void OpenDeleteAccountPanel()
    {
        registrationPanel.SetActive(false);
        loginPanel.SetActive(false);
        EmailVerificationPanel.SetActive(false);
        gamePanel.SetActive(false);
        deleteAccountPanel.SetActive(true);
        exitConfirmationPanel.SetActive(false);

    }

    public void CloseDeleteAccountPanel()
    {
        deleteAccountPanel.SetActive(false);
        exitConfirmationPanel.SetActive(false);
    }

    public void RefreshUserData()
    {
        nameDisplay.text = References.userName;
        emailDisplay.text = References.userEmail;
    }

    public void ShowVerificationResponse(bool isEmailSent, string emailId, string errorMessage)
    {
        //ClearUI();
        EmailVerificationPanel.SetActive(true);

        if (isEmailSent) {
           emailVerificationText.text = $"Please verify your email address\n Verification email has been sent to {emailId}";
        }
        else
        {
            emailVerificationText.text = $"Couldn't send email : {errorMessage}";
        }
    }

    // Open exit confirmation dialog (optional but recommended)
    public void OpenExitConfirmationPanel()
    {
        exitConfirmationPanel.SetActive(true);
    }

    // Close exit confirmation dialog
    public void CloseExitConfirmationPanel()
    {
        exitConfirmationPanel.SetActive(false);
    }

    // Exit game function - Call this from your exit button
    public void ExitGame()
    {
        StartCoroutine(ExitGameCoroutine());
    }

    private IEnumerator ExitGameCoroutine()
    {
        Debug.Log("Exiting game...");

        // Optional: Save game data before exiting
        // You can add saving logic here if needed
        //yield return StartCoroutine(SaveGameData());
        yield return new WaitForSeconds(0.1f);
        // Exit the game
        QuitApplication();
    }

    // Main quit function that handles different platforms
    public void QuitApplication()
    {
        #if UNITY_EDITOR
                    // Application.Quit() doesn't work in the Editor
                    UnityEditor.EditorApplication.isPlaying = false;
        #elif UNITY_WEBGL
                    // For WebGL, show a message (can't quit a webpage)
                    Debug.Log("Cannot quit WebGL application. Please close the browser tab.");
                    if (emailVerificationText != null)
                    {
                        emailVerificationText.text = "Cannot quit WebGL application.\nPlease close the browser tab.";
                        EmailVerificationPanel.SetActive(true);
                    }
        #elif UNITY_ANDROID || UNITY_IOS
                    // For mobile, you might want to minimize instead
                    Debug.Log("Exiting mobile application");
                    Application.Quit();
        #else
                // For standalone builds (Windows, Mac, Linux)
                Application.Quit();
        #endif
    }

    // Alternative: Simple exit function without coroutine
    public void ExitGameSimple()
    {
        Debug.Log("Exiting game...");
        QuitApplication();
    }

    // Force exit without any checks (use with caution)
    public void ForceExitGame()
    {
        #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
        #else
                System.Diagnostics.Process.GetCurrentProcess().Kill(); // Force kill
        #endif
    }

    // Save any unsaved data before exiting (optional)
    private IEnumerator SaveGameDataBeforeExit()
    {
        // Add your save game logic here
        // Example:
        // if (GameManager.Instance != null)
        // {
        //     yield return StartCoroutine(GameManager.Instance.SavePlayerDataCoroutine());
        // }

        Debug.Log("Game data saved before exit");
        yield return null;
    }

    /**************** PASSWORD HIDE*****************/
    // Initialize password visibility toggle buttons
    private void InitializePasswordToggles()
    {
        // Login password toggle
        if (loginPasswordToggleBtn != null && loginPasswordInput != null)
        {
            loginPasswordToggleBtn.onClick.AddListener(() => TogglePasswordVisibility(
                loginPasswordInput, loginPasswordToggleBtn, ref isLoginPasswordVisible
            ));
            UpdatePasswordToggleIcon(loginPasswordToggleBtn, isLoginPasswordVisible);
        }

        // Register password toggle
        if (registerPasswordToggleBtn != null && registerPasswordInput != null)
        {
            registerPasswordToggleBtn.onClick.AddListener(() => TogglePasswordVisibility(
                registerPasswordInput, registerPasswordToggleBtn, ref isRegisterPasswordVisible
            ));
            UpdatePasswordToggleIcon(registerPasswordToggleBtn, isRegisterPasswordVisible);
        }

        // Register confirm password toggle
        if (registerConfirmPasswordToggleBtn != null && registerConfirmPasswordInput != null)
        {
            registerConfirmPasswordToggleBtn.onClick.AddListener(() => TogglePasswordVisibility(
                registerConfirmPasswordInput, registerConfirmPasswordToggleBtn, ref isConfirmPasswordVisible
            ));
            UpdatePasswordToggleIcon(registerConfirmPasswordToggleBtn, isConfirmPasswordVisible);
        }

        // Register confirm password toggle
        if (deleteConfirmPasswordToggleBtn != null && deleteConfirmPasswordInput != null)
        {
            deleteConfirmPasswordToggleBtn.onClick.AddListener(() => TogglePasswordVisibility(
                deleteConfirmPasswordInput, deleteConfirmPasswordToggleBtn, ref isDeleteConfirmPasswordVisible
            ));
            UpdatePasswordToggleIcon(deleteConfirmPasswordToggleBtn, isDeleteConfirmPasswordVisible);
        }
    }

    // Main function to toggle password visibility
    public void TogglePasswordVisibility(TMP_InputField passwordField, Button toggleButton, ref bool isVisible)
    {
        if (passwordField == null) return;

        isVisible = !isVisible;

        if (isVisible)
        {
            // Show password in plain text
            passwordField.contentType = TMP_InputField.ContentType.Standard;
        }
        else
        {
            // Hide password with asterisks
            passwordField.contentType = TMP_InputField.ContentType.Password;
        }

        // Force the input field to update
        passwordField.ForceLabelUpdate();

        // Update the toggle button icon
        UpdatePasswordToggleIcon(toggleButton, isVisible);
    }

    // Update the toggle button icon based on visibility state
    private void UpdatePasswordToggleIcon(Button toggleButton, bool isVisible)
    {
        if (toggleButton == null) return;

        Image buttonImage = toggleButton.GetComponent<Image>();
        if (buttonImage == null) return;

        if (isVisible)
        {
            buttonImage.sprite = visibleIcon;  // Eye open
            if (buttonImage.color.a < 1f)
                buttonImage.color = new Color(buttonImage.color.r, buttonImage.color.g, buttonImage.color.b, 1f);
        }
        else
        {
            buttonImage.sprite = hiddenIcon;   // Eye closed
            if (buttonImage.color.a < 1f)
                buttonImage.color = new Color(buttonImage.color.r, buttonImage.color.g, buttonImage.color.b, 1f);
        }
    }

    // Convenience methods for specific password fields
    public void ToggleLoginPasswordVisibility()
    {
        if (loginPasswordInput != null && loginPasswordToggleBtn != null)
        {
            TogglePasswordVisibility(loginPasswordInput, loginPasswordToggleBtn, ref isLoginPasswordVisible);
        }
    }

    public void ToggleRegisterPasswordVisibility()
    {
        if (registerPasswordInput != null && registerPasswordToggleBtn != null)
        {
            TogglePasswordVisibility(registerPasswordInput, registerPasswordToggleBtn, ref isRegisterPasswordVisible);
        }
    }

    public void ToggleConfirmPasswordVisibility()
    {
        if (registerConfirmPasswordInput != null && registerConfirmPasswordToggleBtn != null)
        {
            TogglePasswordVisibility(registerConfirmPasswordInput, registerConfirmPasswordToggleBtn, ref isConfirmPasswordVisible);
        }
    }

    // Reset all password fields to hidden state
    public void ResetPasswordVisibility()
    {
        ResetPasswordField(loginPasswordInput, loginPasswordToggleBtn, ref isLoginPasswordVisible);
        ResetPasswordField(registerPasswordInput, registerPasswordToggleBtn, ref isRegisterPasswordVisible);
        ResetPasswordField(registerConfirmPasswordInput, registerConfirmPasswordToggleBtn, ref isConfirmPasswordVisible);
        ResetPasswordField(deleteConfirmPasswordInput, deleteConfirmPasswordToggleBtn, ref isDeleteConfirmPasswordVisible);
    }

    private void ResetPasswordField(TMP_InputField passwordField, Button toggleButton, ref bool isVisible)
    {
        if (passwordField != null)
        {
            isVisible = false;
            passwordField.contentType = TMP_InputField.ContentType.Password;
            passwordField.ForceLabelUpdate();

            if (toggleButton != null)
            {
                UpdatePasswordToggleIcon(toggleButton, false);
            }
        }
    }
}
