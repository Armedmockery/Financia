using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("Bars")]
    public Slider healthSlider;
    public Slider EnemyhealthSlider;

    //[Header("Wisdom/Dagger")]
    //public Slider DaggerSlider;

    [Header("Text")]
    public TextMeshProUGUI balanceText;

    [Header("Data Source")]
    public PlayerStats playerStats;
    public EnemyStats enemyStats;

    void Start()
    {
       
        // Set slider limits ONCE
        healthSlider.maxValue = playerStats.maxHealth;
        healthSlider.value = playerStats.currentHealth;

        EnemyhealthSlider.maxValue = enemyStats.maxHealth;
        EnemyhealthSlider.value = enemyStats.currentHealth;


        //DaggerSlider.maxValue = playerStats.maxWisdom;
        //DaggerSlider.value = playerStats.currentWisdom;
    }

    void Update()
    {
        UpdateHealth();
       // UpdateWisdom();
    }

    void UpdateHealth()
    {
        healthSlider.value = playerStats.currentHealth;
        EnemyhealthSlider.value = enemyStats.currentHealth;
    }
    //void UpdateWisdom()
    //{
    //    DaggerSlider.value = playerStats.currentWisdom;
    //}
}
