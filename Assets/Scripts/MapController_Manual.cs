using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class MapController_Manual : MonoBehaviour
{
    public static MapController_Manual instance { get; set; }

    public GameObject mapParent;
    
    List<Image> mapImages;

    public Color highlightColour = Color.blanchedAlmond;
    public Color dimedColour = new Color(1f, 1f, 1f, 0.5f);

    public RectTransform playerIconTransform;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        // SAFETY CHECK
        if (mapParent == null)
        {
            Debug.LogError("Map Parent is NOT assigned!");
            return;
        }

        mapImages = mapParent.GetComponentsInChildren<Image>().ToList();
    }

    public void HighlightArea(MapArea area)
    {
        HighlightArea(area.ToString());
    }

    public void HighlightArea(string areaName)
    {
        foreach (Image area in mapImages)
        {
            area.color = dimedColour;
        }

        Image currentArea = mapImages.Find(x => x.name == areaName);

        if (currentArea != null)
        {
            currentArea.color = highlightColour;

            playerIconTransform.position = currentArea.GetComponent<RectTransform>().position;
            
        }
        else
        {
            Debug.LogWarning("Area not found: " + areaName);
        }
    }
}
        