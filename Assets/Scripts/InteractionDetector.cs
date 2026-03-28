using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour
{
    private IInteractable interactableInRange = null; // Renamed for clarity
    public GameObject interactionIcon;

    void Start()
    {
        if (interactionIcon != null)
            interactionIcon.SetActive(false);
    }

    // Called by the new Input System when the Interact action is performed
    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            TryInteract();
        }
    }

    // Public method that can be called by the UI button
    public void TryInteract()
    {
        if (interactableInRange != null)
        {
            interactableInRange.Interact();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable) && interactable.canInterac())
        {
            interactableInRange = interactable;
            if (interactionIcon != null)
                interactionIcon.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable) && interactable == interactableInRange)
        {
            interactableInRange = null; // FIXED: was incorrectly setting to interactable
            if (interactionIcon != null)
                interactionIcon.SetActive(false);
        }
    }
}