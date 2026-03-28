using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public Transform attackPoint;
    public float weaponRange = 1;
    public float knockbackForce = 50;
    public float stunTime = 1;
    public LayerMask emenyLayer;
    public int damage = 10;


    private Animator animator;
    public Rigidbody2D rb;

    public float jumpForce = 7f;
    private bool isGrounded;

    

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public void OnJumpButtonPressed()
    {
        Debug.Log("PLAYER JUMP");
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        animator.SetTrigger("Jump");
    }

    public void OnHitPressed()
    {
        Debug.Log("Hit");
        animator.SetTrigger("Hit");
        
    }

    public void OnDead()
    {
        Debug.Log("Hit");
        animator.SetTrigger("Dead");

    }

    public void DealDamaage()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(attackPoint.position, weaponRange, emenyLayer);


        if (enemies.Length > 0)
        {
            enemies[0].GetComponent<EnemyStats>().EnemyDamaged(damage);
            enemies[0].GetComponent<Enemy_Knockback>().Knockback(transform , knockbackForce, stunTime);
        }
    }

    public void OnBigHitPressed()
    {
        
            Debug.Log("BigHIt");

            animator.SetTrigger("BigHit");
        
            
    }

    public void OnHurt()
    {
       
        if (SoundEffectManager.Instance != null)
            SoundEffectManager.Instance.PlaySFX("Player Hit");
        Debug.Log("Hurt");
       
        animator.SetTrigger("Hurt");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(attackPoint.position, weaponRange);
    }

 
}
