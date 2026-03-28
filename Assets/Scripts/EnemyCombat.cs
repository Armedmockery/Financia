using UnityEngine;

public class EnemyCombat : MonoBehaviour
{

    public int damage = 10;
    public Transform attackPoint;
    public float weaponRange;
    public float KnockbackForce;
    public LayerMask playerLayer;
    public float stunTime;

    //private void OnCollisionEnter2D(Collision2D collision)

    //{  if(collision.gameObject.tag== "Player")
    //    {
    //        collision.gameObject.GetComponent<PlayerStats>().TakeDamage(damage);
    //    }
       
    //}

    public void Attack()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, weaponRange, playerLayer);



        if (hits.Length > 0)
        {
            hits[0].GetComponent<PlayerStats>().TakeDamage(damage);
            hits[0].GetComponent<PlayerMovement1D>().Knockback(transform, KnockbackForce,stunTime);
            hits[0].GetComponent<PlayerCombat>().OnHurt();

        }
        Debug.Log("Enemy attack hit check");

    }
}
