using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController2D : MonoBehaviour
{

    public float moveSpeed = 5f;

    private bool isKnockedBack = false;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;
    public float footstepInterval = 0.4f;
    public string currentFootstepSFX = "Footsteps"; // default sound
    private float footstepTimer;
public PlayerStats playerStats;

public bool isAttacking = true;

    


    void Start()
    {
        // Initialize the Rigidbody2D component
        rb = GetComponent<Rigidbody2D>();
        // Prevent the player from rotating
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        animator = GetComponent<Animator>();
    }

    void FixedUpdate()
    {


        if (PauseController.IsGamePaused)
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("isWalking", false);
            return;
        }

        if (isKnockedBack)
            return;

        bool isWalking = moveInput != Vector2.zero;

        if (!isAttacking)
        {
            rb.linearVelocity = moveInput * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }

        animator.SetBool("isWalking", rb.linearVelocity.magnitude > 0);
        if (isWalking)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f)
            {
                if (SoundEffectManager.Instance != null)
                    SoundEffectManager.Instance.PlaySFX(currentFootstepSFX);

                footstepTimer = footstepInterval;
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                playerStats.TakeDamage(10);
                playerStats.DecreaseWisdom(10);
            }
            footstepTimer = 0f;
        }

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

    public void Knockback(Transform obj, float force, float stunTime)
    {
        isKnockedBack = true;
        Vector2 direction = (transform.position - obj.position).normalized;
        rb.linearVelocity = direction * force;
        Debug.Log("knocked back");
        StartCoroutine(KnockbackCounter(stunTime));
    }

    IEnumerator KnockbackCounter(float stunTime)
    {
        yield return new WaitForSeconds(stunTime);
        rb.linearVelocity = Vector2.zero;
        isKnockedBack = false;
    }



}
// using UnityEngine;
// using UnityEngine.InputSystem;

// public class PlayerController2D : MonoBehaviour
// {
//     public float moveSpeed = 5f;
//     public float footstepInterval = 0.4f;

//     private Rigidbody2D rb;
//     private Vector2 moveInput;
//     private Animator animator;
//     private float footstepTimer;

//     void Awake() // use Awake for safety
//     {
//         rb = GetComponent<Rigidbody2D>();
//         animator = GetComponent<Animator>();

//         rb.constraints = RigidbodyConstraints2D.FreezeRotation;
//     }

//     void Update()
//     {
//         rb.velocity = moveInput * moveSpeed;

//         bool isWalking = moveInput != Vector2.zero;
//         animator.SetBool("isWalking", isWalking);

//         if (isWalking)
//         {
//             footstepTimer -= Time.deltaTime;

//             if (footstepTimer <= 0f)
//             {
//                 if (SoundEffectManager.Instance != null)
//                 {
//                     SoundEffectManager.Instance.Play("Footsteps");
//                 }
//                 footstepTimer = footstepInterval;
//             }
//         }
//         else
//         {
//             footstepTimer = 0f;
//         }
//     }

//     public void Move(InputAction.CallbackContext context)
//     {
//         if (context.canceled)
//         {
//             moveInput = Vector2.zero;

//             animator.SetFloat("LastInputX", animator.GetFloat("InputX"));
//             animator.SetFloat("LastInputY", animator.GetFloat("InputY"));
//             return;
//         }

//         moveInput = context.ReadValue<Vector2>();
//         animator.SetFloat("InputX", moveInput.x);
//         animator.SetFloat("InputY", moveInput.y);
//     }
// }
