using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    private Transform originalParent;
    private CanvasGroup canvasGroup;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;          // Save original slot
        transform.SetParent(transform.root);         // Bring item above UI
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;                    // Semi-transparent
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;    // Follow pointer
    }

    public void OnEndDrag(PointerEventData eventData)
{
    canvasGroup.blocksRaycasts = true;
    canvasGroup.alpha = 1f;

    GameObject target = eventData.pointerEnter;

    Slot originalSlot = originalParent.GetComponent<Slot>();

    if (target == null)
    {
        ReturnToOriginal();
        return;
    }

    HotbarSlot hotbarTarget =
        target.GetComponentInParent<HotbarSlot>();

    if (hotbarTarget != null)
    {
        hotbarTarget.linkedInventorySlot = originalSlot;
        ReturnToOriginal();
        return;
    }

    Slot dropSlot = target.GetComponentInParent<Slot>();

    if (dropSlot != null)
    {
        if (dropSlot.currentItem != null)
        {
            dropSlot.currentItem.transform.SetParent(originalSlot.transform);
            originalSlot.currentItem = dropSlot.currentItem;
            dropSlot.currentItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
        else
        {
            originalSlot.currentItem = null;
        }

        transform.SetParent(dropSlot.transform);
        dropSlot.currentItem = gameObject;
        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }
    else
    {
        ReturnToOriginal();
    }
}

void ReturnToOriginal()
{
    transform.SetParent(originalParent);
    GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
}

}
