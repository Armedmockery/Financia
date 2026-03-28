using UnityEngine;
using UnityEngine.EventSystems;

public class HotbarSlotClick : MonoBehaviour, IPointerClickHandler
{
    public int slotIndex;
    private HotbarController hotbarController;

    void Start()
    {
        hotbarController = FindObjectOfType<HotbarController>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        hotbarController.UseItemInSlot(slotIndex);
    }
}
