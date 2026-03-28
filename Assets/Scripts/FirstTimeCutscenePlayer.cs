using System.Collections;
using UnityEngine;
using Firebase.Firestore;

public class FirstTimeCutscenePlayer : MonoBehaviour
{
    [SerializeField] private CutSceneManager cutsceneManager;
    [SerializeField] private string tutorialVideoFileName = "Tutorial.mp4";
    [SerializeField] private string introCutsceneFileName = "Untitled design(1).mp4";

    IEnumerator Start()
    {
        // Wait until FirebaseAuthManager is ready and user is loaded
        yield return new WaitUntil(() =>
            FirebaseAuthManager.Instance != null &&
            FirebaseAuthManager.Instance.db != null &&
            FirebaseAuthManager.Instance.user != null
        );

        FirebaseFirestore db = FirebaseAuthManager.Instance.db;
        string userId = FirebaseAuthManager.Instance.user.UserId;

        DocumentReference userDoc = db.Collection("users").Document(userId);

        // Fetch the user document
        var fetchTask = userDoc.GetSnapshotAsync();
        yield return new WaitUntil(() => fetchTask.IsCompleted);

        if (fetchTask.Exception != null)
        {
            Debug.LogWarning("FirstTimeCutscenePlayer: Could not fetch user doc - " + fetchTask.Exception);
            yield break;
        }

        DocumentSnapshot snapshot = fetchTask.Result;

        bool isFirstTime = false;

        if (snapshot.Exists && snapshot.ContainsField("isFirstTimePlaying"))
        {
            isFirstTime = snapshot.GetValue<bool>("isFirstTimePlaying");
        }

        if (!isFirstTime)
        {
            Debug.Log("Not first time playing, skipping intro videos.");
            yield break;
        }

        // --- Play Tutorial First ---
        Debug.Log("Playing tutorial video...");
        cutsceneManager.PlayCutsceneFromFile(tutorialVideoFileName);

        // Wait for tutorial to finish
        yield return new WaitUntil(() => cutsceneManager.IsPlaying);   // wait for it to start
        yield return new WaitUntil(() => !cutsceneManager.IsPlaying);  // wait for it to end

        // --- Play Intro Cutscene Second ---
        Debug.Log("Playing intro cutscene...");
        cutsceneManager.PlayCutsceneFromFile(introCutsceneFileName);

        // Wait for cutscene to finish
        yield return new WaitUntil(() => cutsceneManager.IsPlaying);
        yield return new WaitUntil(() => !cutsceneManager.IsPlaying);

        // --- Mark as done so neither ever plays again ---
        var updateTask = userDoc.UpdateAsync("isFirstTimePlaying", false);
        yield return new WaitUntil(() => updateTask.IsCompleted);

        if (updateTask.Exception != null)
            Debug.LogWarning("Could not update isFirstTimePlaying: " + updateTask.Exception);
        else
            Debug.Log("First time sequence complete, flag set to false.");
    }
}