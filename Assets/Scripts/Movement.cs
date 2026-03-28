using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{

    public float moveSpeed = 5f;


    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;
    void Start()
    {
        // Initialize the Rigidbody2D component
        rb = GetComponent<Rigidbody2D>();
        // Prevent the player from rotating
        //rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        rb.linearVelocity = Vector2.zero;
    }

    public void Move(InputAction.CallbackContext context)
    {

        if (context.canceled)
        {
            animator.SetBool("isWalking", false);
            animator.SetFloat("LastInputX", moveInput.x);
            animator.SetFloat("LastInputY", moveInput.y);
        }
        moveInput = context.ReadValue<Vector2>();


        animator.SetFloat("InputX", moveInput.x);
        animator.SetFloat("InputY", moveInput.y);



    }
}
