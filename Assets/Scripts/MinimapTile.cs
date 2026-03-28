using UnityEngine;
using UnityEngine.UI;

public class MinimapTile : MonoBehaviour
{
    public Image background;  // Always visible
    public Image border;      // Your colored border image
    public Image icon;        // Changes based on room type

    public void SetIcon(Sprite sprite)
    {
        if (icon != null)
        {
            icon.sprite = sprite;
            icon.gameObject.SetActive(sprite != null);
        }
    }

    public void SetCurrent(bool value)
    {
        if (border != null)
        {
            border.gameObject.SetActive(value); // Show border only for current room
        }
    }
}