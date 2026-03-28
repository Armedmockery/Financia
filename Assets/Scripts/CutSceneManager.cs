using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections;
using System.IO;

public class CutSceneManager : MonoBehaviour
{
    [Header("Cutscene Settings")]
    [SerializeField] private GameObject cutsceneCanvas;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage videoDisplay;

    [Header("Cutscene Videos")]
    [SerializeField] private VideoClip firstLoginCutscene;
    [SerializeField] private VideoClip[] otherCutscenes;
    [SerializeField] private string videoFolderPath = "Videos";

    [Header("Skip Button")]
    [SerializeField] private Button skipButton;

    private bool hasPlayedFirstCutscene = false;
    private const string FIRST_PLAY_KEY = "FirstTimePlaying";
    public bool IsPlaying => cutsceneCanvas != null && cutsceneCanvas.activeSelf;
    // Add this anywhere in CutSceneManager
    public static bool HasPendingPhoneNotification { get; private set; }

    void Start()
    {
        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.errorReceived += OnVideoError;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.controlledAudioTrackCount = 1;
        videoPlayer.skipOnDrop = true;
        videoPlayer.playOnAwake = false;
        if (cutsceneCanvas != null)
            cutsceneCanvas.SetActive(false);

        if (skipButton != null)
        {
            skipButton.onClick.AddListener(SkipCutscene);
            skipButton.gameObject.SetActive(false);
        }
    }

    void OnVideoError(VideoPlayer source, string message)
    {
        Debug.LogError($"Video Error: {message}");
        SkipCutscene();
    }

    public void PlayCutscene(VideoClip clip, bool notifyPhone = false)
    {
        if (clip == null)
        {
            Debug.LogWarning("No video clip assigned!");
            return;
        }

        StartCoroutine(PlayCutsceneRoutine(clip, notifyPhone));
    }

    public void PlayCutsceneByIndex(int cutsceneIndex)
    {
        if (cutsceneIndex >= 0 && cutsceneIndex < otherCutscenes.Length)
        {
            PlayCutscene(otherCutscenes[cutsceneIndex]);
        }
        else
        {
            Debug.LogError($"Cutscene index {cutsceneIndex} out of range! Total cutscenes: {otherCutscenes.Length}");
        }
    }

    public void PlayCutsceneFromFile(string fileName)
    {
        if (!fileName.EndsWith(".mp4") && !fileName.EndsWith(".webm") && !fileName.EndsWith(".mov"))
        {
            fileName += ".mp4";
        }

        string streamingAssetsPath = Application.streamingAssetsPath;
        string fullPath = Path.Combine(streamingAssetsPath, videoFolderPath, fileName);
        string fileURL = "file://" + fullPath.Replace("\\", "/");

        Debug.Log($"Attempting to play video from: {fileURL}");

        if (File.Exists(fullPath))
        {
            Debug.Log($"File found at: {fullPath}");
            StartCoroutine(PlayCutsceneFromPathRoutine(fileURL));
        }
        else
        {
            Debug.LogError($"Video file not found at: {fullPath}");

            string alternativePath = Path.Combine(Application.dataPath, "Videos", fileName);
            if (File.Exists(alternativePath))
            {
                Debug.Log($"Found at alternative path: {alternativePath}");
                string altURL = "file://" + alternativePath.Replace("\\", "/");
                StartCoroutine(PlayCutsceneFromPathRoutine(altURL));
            }
        }
    }

    IEnumerator PlayCutsceneRoutine(VideoClip clip, bool notifyPhone)
    {
        Time.timeScale = 0f;
        ShowCutscene(); // ← CHANGED from cutsceneCanvas.SetActive(true)

        videoPlayer.source = VideoSource.VideoClip;
        videoPlayer.clip = clip;
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        videoPlayer.Play();

        while (videoPlayer.isPlaying)
        {
            yield return null;
        }

        HideCutscene(); // ← CHANGED from cutsceneCanvas.SetActive(false)
        Time.timeScale = 1f;

        if (notifyPhone)
            HasPendingPhoneNotification = true;
    }

    IEnumerator PlayCutsceneFromPathRoutine(string fileURL)
    {
        if (cutsceneCanvas.activeSelf)
            HideCutscene(); // ← CHANGED from cutsceneCanvas.SetActive(false)
        yield return null;

        Time.timeScale = 0f;
        ShowCutscene(); // ← CHANGED from cutsceneCanvas.SetActive(true)

        videoPlayer.Stop();
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = fileURL;
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        videoPlayer.skipOnDrop = true;

        videoPlayer.Prepare();

        float timeout = 10f;
        float elapsedTime = 0f;

        while (!videoPlayer.isPrepared && elapsedTime < timeout)
        {
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        if (videoPlayer.isPrepared)
        {
            Debug.Log("Video prepared successfully, playing...");
            videoPlayer.Play();

            while (videoPlayer.isPlaying)
            {
                yield return null;
            }

            Debug.Log("Video finished playing");
        }
        else
        {
            Debug.LogError("Video preparation timed out!");
        }

        HideCutscene(); // ← CHANGED from cutsceneCanvas.SetActive(false)
        Time.timeScale = 1f;
        videoPlayer.Stop();
        videoPlayer.url = null;
        videoPlayer.source = VideoSource.VideoClip;
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("Cutscene finished!");
        HasPendingPhoneNotification = true;
    }

    public void SkipCutscene()
    {
        if (cutsceneCanvas.activeSelf)
        {
            videoPlayer.Stop();
            HideCutscene(); // ← CHANGED from cutsceneCanvas.SetActive(false)
            Time.timeScale = 1f;
        }
    }

    // ← ADDED: shows canvas AND skip button together
    private void ShowCutscene()
    {
        cutsceneCanvas.SetActive(true);
        if (skipButton != null)
            skipButton.gameObject.SetActive(true);
    }

    // ← ADDED: hides canvas AND skip button together
    private void HideCutscene()
    {
        cutsceneCanvas.SetActive(false);
        if (skipButton != null)
            skipButton.gameObject.SetActive(false);
    }

    void Update()
    {
        if (cutsceneCanvas.activeSelf && Input.GetKeyDown(KeyCode.Space))
        {
            SkipCutscene();
        }
    }

    public static void ClearPhoneNotification()
    {
        HasPendingPhoneNotification = false;
    }
}