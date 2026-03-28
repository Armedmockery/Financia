using UnityEngine;

public class EnemyCombatTopDown : MonoBehaviour
{
    public int damage = 10;
    public Transform attackPoint;
    public float weaponRange = 0.8f;
    public LayerMask playerLayer;
    public Sprite sprite;

    public void Attack()
    {
        Debug.Log("Enemy attack triggered");

        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                attackPoint.position,
                weaponRange,
                playerLayer
            );
        bool damageDealt = false; // Track if we actually damaged player

        foreach (Collider2D hit in hits)
        {
            PlayerStats stats = hit.GetComponent<PlayerStats>();
            if (stats != null)
        {
            int healthBefore = stats.currentHealth; // Assuming you have this
            stats.TakeDamage(damage);
            
            // Check if health actually decreased
            if (stats.currentHealth < healthBefore)
            {
                damageDealt = true;
            }
        }
        }
        // Only show popup if damage was actually dealt
    if (damageDealt && ItemPickupUIController.Instance != null)
    {
        ItemPickupUIController.Instance.ShowItemPickup("Health -" + damage, sprite);
    }
    else
    {
        Debug.Log("Something else is the issue ");
    }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, weaponRange);
    }
}
