using UnityEngine;

public class PlayerCombatTopDown : MonoBehaviour
{
    public Transform attackPoint;
    public float attackRange = 1f;
    public int damage = 10;
    public LayerMask enemyLayer;

    private Animator animator;
    private PlayerController2D controller;

    void Awake()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<PlayerController2D>();
    }
    void Update()
{
    if (Input.GetMouseButtonDown(0))
    {
        Attack();
    }
}
void LateUpdate()
{
    float x = animator.GetFloat("LastInputX");
    float y = animator.GetFloat("LastInputY");

    Vector2 dir = new Vector2(x, y).normalized;

    if (dir != Vector2.zero)
        attackPoint.localPosition = dir * 0.8f;
}


    public void Attack()
    {
        controller.isAttacking = true;
        animator.SetTrigger("Attack");
    }


    public void DealDamage()
    {
        Collider2D[] enemies =
            Physics2D.OverlapCircleAll(
                attackPoint.position,
                attackRange,
                enemyLayer
            );
            Debug.Log("PLAYER ATTACK CHECK");


        

        foreach (Collider2D hit in enemies)
{
    Debug.Log("HIT OBJECT: " + hit.name);

    EnemyStatsMaze stats = hit.GetComponent<EnemyStatsMaze>();

    if (stats == null)
    {
        Debug.Log("EnemyStatsMaze NOT FOUND on: " + hit.name);
    }
    else
    {
        Debug.Log("EnemyStatsMaze FOUND on: " + hit.name);
        stats.EnemyDamaged(damage);
    }
}


        }
       
    public void OnDead()
{
    animator.SetTrigger("Dead");
}


    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
    public void EndAttack()
{
    controller.isAttacking = false;
}

}
