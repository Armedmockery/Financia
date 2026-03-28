using UnityEngine;

public class CutSceneTest : MonoBehaviour
{
    [SerializeField] private CutSceneManager cutsceneManager;
    [SerializeField] private int cutsceneIndex = 0;
    [SerializeField] private string videoFileName = "Untitled design(1).mp4"; // Include extension!
    [SerializeField] private bool playOnce = true;
    [SerializeField] private string triggerTag = "Player";

    private bool hasPlayed = false;

    void Start()
    {
        // Verify the video file exists at start
        if (!string.IsNullOrEmpty(videoFileName))
        {
            string streamingPath = System.IO.Path.Combine(Application.streamingAssetsPath, "Videos", videoFileName);
            string assetsPath = System.IO.Path.Combine(Application.dataPath, "Videos", videoFileName);

            if (System.IO.File.Exists(streamingPath))
            {
                Debug.Log($"Video file found in StreamingAssets: {streamingPath}");
            }
            else if (System.IO.File.Exists(assetsPath))
            {
                Debug.Log($"Video file found in Assets: {assetsPath}");
            }
            else
            {
                Debug.LogWarning($"Video file not found: {videoFileName}");
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(triggerTag))
        {
            if (playOnce && hasPlayed)
                return;

            if (cutsceneManager != null)
            {
                Debug.Log($"Trigger activated! Playing cutscene: {(string.IsNullOrEmpty(videoFileName) ? "Index " + cutsceneIndex : videoFileName)}");

                if (!string.IsNullOrEmpty(videoFileName))
                {
                    cutsceneManager.PlayCutsceneFromFile(videoFileName);
                }
                else
                {
                    cutsceneManager.PlayCutsceneByIndex(cutsceneIndex);
                }

                hasPlayed = true;
            }
            else
            {
                Debug.LogError("CutsceneManager not assigned to trigger!");
            }
        }
    }
}