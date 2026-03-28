using System.Collections;
using UnityEngine;

public class Enemy_Knockback : MonoBehaviour
{
    private Rigidbody2D rb;
    private EnemyMovement enemy_Movement;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        enemy_Movement = GetComponent<EnemyMovement>();
    }

    public void Knockback(Transform playerTransform , float knockbackForce, float stunTime)
    {
        enemy_Movement.ChangeState(EnemyState.Knockback);
        StartCoroutine(StunTimer(stunTime));
        Vector2 direction =(transform.position - playerTransform.position).normalized;
        rb.linearVelocity = knockbackForce * direction;
        Debug.Log("demon knockedback");
    }

    IEnumerator StunTimer (float stuntime)
    {
        yield return new WaitForSeconds(stuntime);
        rb.linearVelocity = Vector2.zero;
        enemy_Movement.ChangeState(EnemyState.Idle);
    }
    
}
