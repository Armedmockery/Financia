using UnityEngine;

public class Damager: MonoBehaviour
{
    public int damage = 10;
    public float KnockbackForce;
    public float stunTime;
    public Sprite sprite;

    void OnCollisionEnter2D(Collision2D collision)
    {
        var stats = collision.gameObject.GetComponent<PlayerStats>();
        var controller = collision.gameObject.GetComponent<PlayerController2D>();
        

        if (stats != null && controller != null)
        {
            stats.TakeDamage(damage);
            controller.Knockback(transform, KnockbackForce, stunTime);
            
    
            ItemPickupUIController.Instance.ShowItemPickup("Health -" + damage ,sprite);
        }
    }


}
