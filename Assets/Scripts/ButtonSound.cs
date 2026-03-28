using UnityEngine;

public class ButtonSound : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private string musicName;

    public void PlaySFX()
    {
        if (string.IsNullOrEmpty(musicName))
        {
            Debug.LogWarning($"[ButtonBGMTrigger] Music name is empty on {gameObject.name}!");
            return;
        }

        SoundEffectManager.Instance.PlaySFX(musicName);
    }
}
