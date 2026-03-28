using UnityEngine;

public class EnemyMovementTopDown : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float attackRange = 1.2f;
    public float detectionRange = 6f;
    public float attackCooldown = 1.5f;

    public Transform attackPoint;
    public float attackOffset = 0.6f;


    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;

    private float attackTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (player == null)
        {
            FindPlayer();
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            rb.linearVelocity = Vector2.zero;
            TryAttack();
        }
        else if (distance <= detectionRange)
        {
            ChasePlayer();
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetFloat("Speed", 0);
        }
    }

    void FindPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;
    }

    void ChasePlayer()
    {
        Vector2 dir = (player.position - transform.position).normalized;
        rb.linearVelocity = dir * moveSpeed;

        animator.SetFloat("MoveX", dir.x);
        animator.SetFloat("MoveY", dir.y);
        animator.SetFloat("Speed", rb.linearVelocity.magnitude);

        UpdateAttackPoint(dir);
    }

    void UpdateAttackPoint(Vector2 dir)
    {
        if (attackPoint == null) return;

        Vector2 snappedDir;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            snappedDir = new Vector2(Mathf.Sign(dir.x), 0);
        else
            snappedDir = new Vector2(0, Mathf.Sign(dir.y));

        attackPoint.localPosition = snappedDir * attackOffset;
    }


    void TryAttack()
    {
        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0f)
        {
            attackTimer = attackCooldown;
            animator.SetTrigger("Attack");
        }
    }
}
