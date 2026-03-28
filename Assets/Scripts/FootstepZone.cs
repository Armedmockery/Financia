using UnityEngine;

public class FootstepZone : MonoBehaviour
{
    [SerializeField] private string footstepSFXName = "Footsteps";
    [SerializeField] private string defaultFootstepSFXName = "Footsteps";

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController2D player = other.GetComponent<PlayerController2D>();
        if (player != null)
        {
            player.currentFootstepSFX = footstepSFXName;
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerController2D player = other.GetComponent<PlayerController2D>();
        if (player != null)
        {
            player.currentFootstepSFX = defaultFootstepSFXName;
        }
    }
}