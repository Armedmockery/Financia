using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    [Header("Loading Screen Prefab")]
    [SerializeField] private GameObject loadingScreenPrefab;

    private GameObject loadingScreenInstance;
    private Slider progressSlider;
    private TextMeshProUGUI progressText;
    private bool isLoading = false;

    public static SceneLoader Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);      // Persist across scenes
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void LoadGameScene(string sceneName)
    {
        if (!isLoading)
        {
            StartCoroutine(LoadSceneAsync(sceneName));
        }
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        isLoading = true;

        // Instantiate and show loading screen
        loadingScreenInstance = Instantiate(loadingScreenPrefab);
        loadingScreenInstance.SetActive(true);
        DontDestroyOnLoad(loadingScreenInstance);

        progressSlider = loadingScreenInstance.GetComponentInChildren<Slider>();
        progressText = loadingScreenInstance.GetComponentInChildren<TextMeshProUGUI>();

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            if (progressSlider != null)
                progressSlider.value = progress;
            if (progressText != null)
                progressText.text = (progress * 100f).ToString("F0") + "%";

            if (operation.progress >= 0.9f)
            {
                operation.allowSceneActivation = true;
            }

            yield return null;
        }

        // DO NOT destroy here – let the sceneLoaded event handle it.
        // The loading screen will be destroyed in OnSceneLoaded.
        // But we set isLoading false there.
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Find it even if created by a previous scene's SceneLoader
        GameObject existing = GameObject.FindGameObjectWithTag("LoadingScreen");
        if (existing != null)
            Destroy(existing);


        isLoading = false;

        if (scene.name == "Test Prithvi" && SaveController.LoadOnNextGameSceneLoad)
            StartCoroutine(DelayedLoad());
    }

    private IEnumerator DelayedLoad()
    {
        yield return null; // Wait one frame for all Start() to complete
        SaveController.Instance.LoadGame();
        SaveController.LoadOnNextGameSceneLoad = false;
    }
}