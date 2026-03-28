using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerFightActions : MonoBehaviour
{
    private _1DControls controls;
    private PlayerCombat combat;


    void Awake()
    {
        controls = new _1DControls();
        combat = GetComponent<PlayerCombat>(); // <-- ADD THIS
    }

    void OnEnable()
    {
        controls.Player.Enable();
        // Subscribe to the action events
        controls.Player.Jump.performed += OnJump;
        controls.Player.Hit.performed += OnHit;
       // controls.Player.Block.performed += OnBlock;
    }

    void OnDisable()
    {
        controls.Player.Disable();
        controls.Player.Jump.performed -= OnJump;
        controls.Player.Hit.performed -= OnHit;
       // controls.Player.Block.performed -= OnBlock;
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            combat.OnJumpButtonPressed();
        }
    }

    private void OnHit(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            combat.OnHitPressed();
        }
    }

   

   
}