using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Rigidbody2D rb;
    private Animator animator;

    private Vector2 input;
    private Vector2 lastDir = Vector2.down; // default facing down (new Vector2(0,-1) he pn lhiu shkto )

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (PauseController.IsGamePaused) {
            input = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
            animator.SetFloat("Speed", 0f);
            return;
        }
        // read input
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");
        
        // normalize so diagonals don't exceed 1
        if (input.sqrMagnitude > 1f) input = input.normalized;

        // update last non-zero direction when moving
        if (input != Vector2.zero)
        {
            lastDir = input.normalized; //jar player halat asel tr tyache inputs lakshat thevel and store krel lastDir madhe which will help us for direction.
        }

        // always update animator parameters (even when zero)
        animator.SetFloat("Horizontal", input.x);
        animator.SetFloat("Vertical", input.y); //ithe apan animator la input detoy je walking animations movement blend tree sathi vapartoy 
        animator.SetFloat("Speed", input.magnitude);//magnitude bhetta i.e 1 

        // Set last facing direction for Idle tree (use rounded integers if that's your setup)
        animator.SetFloat("Horizontal", lastDir.x);
        animator.SetFloat("Vertical", lastDir.y);//apan he idle blend tree sathi vapartoy so that last direction ji hoti ti maintain hoil 
    }

    void FixedUpdate()
    {
        if (PauseController.IsGamePaused)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }  //actual pasuing
        rb.MovePosition(rb.position + input * moveSpeed * Time.fixedDeltaTime);
    }
}
