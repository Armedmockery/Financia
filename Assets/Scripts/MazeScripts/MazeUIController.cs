using UnityEngine;
using TMPro;
using System.Collections;

public class MazeUIController : MonoBehaviour
{
    public static MazeUIController Instance;

    [Header("UI Text")]
    public TextMeshProUGUI enemyCountText;
    public TextMeshProUGUI enemyTypeText;

    [Header("Settings")]
    public float clearMessageDuration = 2f;

    private Coroutine hideCoroutine;

    private void Awake()
    {
        Instance = this;
        HideUIImmediate();
    }

    public void ShowCombatUI(int remaining, int total, string type)
    {
        StopHideRoutine();

        enemyCountText.text = $"Slimes: {remaining} / {total}";
        enemyTypeText.text = $"Type: {type} Slime";

        enemyCountText.gameObject.SetActive(true);
        enemyTypeText.gameObject.SetActive(true);
    }

    public void UpdateEnemyCount(int remaining, int total)
    {
        enemyCountText.text = $"Slimes: {remaining} / {total}";
    }

    public void ShowRoomCleared()
    {
        StopHideRoutine();

        enemyCountText.text = "Room Cleared! Good Job";
        enemyTypeText.text = "";

        enemyCountText.gameObject.SetActive(true);
        enemyTypeText.gameObject.SetActive(false);

        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    IEnumerator HideAfterDelay()
{
    yield return new WaitForSeconds(clearMessageDuration);

    float fadeTime = 0.5f;
    float t = 0f;

    Color countColor = enemyCountText.color;
    Color typeColor = enemyTypeText.color;

    while (t < fadeTime)
    {
        t += Time.deltaTime;
        float alpha = Mathf.Lerp(1f, 0f, t / fadeTime);

        enemyCountText.color = new Color(countColor.r, countColor.g, countColor.b, alpha);
        enemyTypeText.color = new Color(typeColor.r, typeColor.g, typeColor.b, alpha);

        yield return null;
    }

    // Reset alpha for next use
    enemyCountText.color = new Color(countColor.r, countColor.g, countColor.b, 1f);
    enemyTypeText.color = new Color(typeColor.r, typeColor.g, typeColor.b, 1f);

    HideUIImmediate();
}
IEnumerator HideAfterDelayCustom(float delay)
{
    yield return new WaitForSeconds(delay);

    float fadeTime = 0.5f;
    float t = 0f;

    Color countColor = enemyCountText.color;

    while (t < fadeTime)
    {
        t += Time.deltaTime;
        float alpha = Mathf.Lerp(1f, 0f, t / fadeTime);

        enemyCountText.color =
            new Color(countColor.r, countColor.g, countColor.b, alpha);

        yield return null;
    }

    enemyCountText.color =
        new Color(countColor.r, countColor.g, countColor.b, 1f);

    HideUIImmediate();
}



    void StopHideRoutine()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
    }

    public void HideUIImmediate()
    {
        enemyCountText.gameObject.SetActive(false);
        enemyTypeText.gameObject.SetActive(false);
    }
    public void ShowCustomMessage(string message, float duration = 3f)
    {
        StopHideRoutine();

        enemyCountText.text = message;
        enemyTypeText.text = "";

        enemyCountText.gameObject.SetActive(true);
        enemyTypeText.gameObject.SetActive(false);

        hideCoroutine = StartCoroutine(HideAfterDelayCustom(duration));
    }

}
