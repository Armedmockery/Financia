using System.Collections;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Wisdom / Dagger")]
    public int maxWisdom = 100;
    public int currentWisdom;

    [Header("Balance")]
    public int balance = 500;

    public bool isDead = false;

    private Animator animator;
    private PlayerCombat PlayerCombat;
    private PlayerCombatTopDown playerCombatTopDown; //its just for maze part , no changes done to the existing part 
    void Awake()
    {
        currentHealth = maxHealth;
        currentWisdom = maxWisdom;

        animator = GetComponentInChildren<Animator>();
        PlayerCombat = GetComponent<PlayerCombat>();
        playerCombatTopDown = GetComponent<PlayerCombatTopDown>();
    }

    // ---------------- HEALTH ----------------
    public void TakeDamage(int amount)
    {
        if (isDead) return;
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (currentHealth == 0)
        {
            isDead=true;
            OnHealthDepleted();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    // ---------------- WISDOM ----------------
    public void IncreaseWisdom(int amount)
    {
        currentWisdom += amount;
        currentWisdom = Mathf.Clamp(currentWisdom, 0, maxWisdom);
    }

    public void DecreaseWisdom(int amount)
    {
        currentWisdom -= amount;
        currentWisdom = Mathf.Clamp(currentWisdom, 0, maxWisdom);
    }

    // ---------------- BALANCE ----------------
    public void AddMoney(int amount)
    {
        balance += amount;
    }

    public bool SpendMoney(int amount)
    {
        if (balance < amount)
            return false;

        balance -= amount;
        return true;
    }

    void OnHealthDepleted()
    {
        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
       if (PlayerCombat!= null)
            PlayerCombat.OnDead(); //ithe changes ahet fkt 

        if (playerCombatTopDown != null)
            playerCombatTopDown.OnDead();
   
        Debug.Log("Player overwhelmed (Game Over state)");

        yield return new WaitForSeconds(1.5f);

        if (MazeLifeManager.Instance != null)
        {
            MazeLifeManager.Instance.OnPlayerDeath(this);
        }
        else
        {
            gameObject.SetActive(false);
            SaveController.Instance?.PersistCurrentState();
            SaveController.LoadOnNextGameSceneLoad = true;
            FindObjectOfType<SceneLoader>().LoadGameScene("Test Prithvi");// fallback for non-maze scenes
        }

    }
}
