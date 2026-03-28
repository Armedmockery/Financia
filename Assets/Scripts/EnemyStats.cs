using System.Collections;
using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [Header("Enemy Identification")]
    public string enemyID;

    [Header("Health")]
    public int maxHealth = 500;
    public int currentHealth;
    private EnemyMovement enemy_Movement;
    private Animator animator;

    [Header("Quest Association")]
    public string requiredQuestID;            // empty = always active

    [Header("Death Cutscene")]
    [SerializeField] private string deathCutsceneFileName = ""; // leave empty for no cutscene

    void Awake()
    {
        currentHealth = maxHealth;
        enemy_Movement = GetComponent<EnemyMovement>();
        animator = GetComponentInChildren<Animator>();

    }

    private void Start()
    {
        //if (!string.IsNullOrEmpty(requiredQuestID) ||
        //(SaveController.Instance != null && SaveController.Instance.IsEnemyDefeated(enemyID)))
        //{
        //    // Hide initially, then check after load completes
        //    gameObject.SetActive(false);
        //    StartCoroutine(CheckActiveAfterLoad());
        //}
        StartCoroutine(CheckActiveAfterLoad());
    }

    private IEnumerator CheckActiveAfterLoad()
    {
        // Wait same number of frames as LoadGameDelayed
        yield return null;
        yield return null;
        yield return null; // extra frame for safety

        //// Check quest requirement
        //if (!string.IsNullOrEmpty(requiredQuestID))
        //{
        //    bool questActive = QuestController.Instance != null &&
        //                       QuestController.Instance.IsQuestActive(requiredQuestID);
        //    if (!questActive)
        //    {
        //        Debug.Log("Monster set false");
        //        gameObject.SetActive(false);
        //        yield break;
        //    }
        //}

        // Check if permanently defeated
        if (SaveController.Instance != null && SaveController.Instance.IsEnemyDefeated(enemyID))
        {
            Debug.Log("Monster set false");
            gameObject.SetActive(false);
            yield break;
        }

        // All checks passed — enable enemy
        Debug.Log("Monster set true");
        gameObject.SetActive(true);
    }

    // ---------------- HEALTH ----------------
    public void EnemyDamaged(int amount)
    {
        if (SoundEffectManager.Instance != null)
            SoundEffectManager.Instance.PlaySFX("Monster Hit");
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (currentHealth == 0)
        {
            
            OnHealthDepleted();
        }
        
    } 

    void OnHealthDepleted()
    {
        // Notify quest system that this enemy type has been defeated
        //if (QuestController.Instance != null)
        //{
        //    QuestController.Instance.UpdateEnemyKill(enemyID, 1);
            SaveController.Instance?.AddDefeatedEnemy(enemyID);
        //}
        QuestController.Instance?.UpdateEnemyKill(enemyID, 1);
        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        animator.SetBool("isDead", true);
        yield return new WaitForSeconds(2f);

        gameObject.SetActive(false);

        SaveController.Instance?.PersistCurrentState();
        SaveController.LoadOnNextGameSceneLoad = true;
        FindObjectOfType<SceneLoader>().LoadGameScene("Test Prithvi");
    }
}
