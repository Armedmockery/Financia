using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement1D : MonoBehaviour
{
    public Rigidbody2D rb;
    public float moveSpeed = 5f;
    public int facingDirection = 1;

    private bool isKnockedBack;
    private float horizontalMovement;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isKnockedBack == false) 
        {
            //if (horizontalMovement > 0 && transform.localScale.x < 0 || horizontalMovement < 0 && transform.localScale.x > 0)
            //{
            //    Flip();
            //}
            rb.linearVelocity = new Vector2(horizontalMovement * moveSpeed, rb.linearVelocity.y);
        }
        
    }

    void Flip()
    {
        facingDirection *= -1;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }

    public void Move(InputAction.CallbackContext context)
    {
        float x = context.ReadValue<Vector2>().x;

        horizontalMovement = x;
        animator.SetFloat("InputX", x);
    }

    public void Knockback( Transform emeny, float force, float stunTime)
    {
        isKnockedBack = true;
        Vector2 direction = (transform.position - emeny.position).normalized;
        rb.linearVelocity = direction * force;
        StartCoroutine(KnockbackCounter(stunTime));
    }

    IEnumerator KnockbackCounter(float stunTime)
    {
        yield return new WaitForSeconds(stunTime);
        rb.linearVelocity = Vector2.zero;
        isKnockedBack = false;
    }
}
