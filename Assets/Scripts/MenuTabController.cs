using UnityEngine;

public class MenuTabsController : MonoBehaviour
{
    [SerializeField] private GameObject[] pages;
    private int currentIndex = 0;

    void Start()
    {
        ShowPage(0);
    }

    public void ShowPage(int index)
    {
        if (index < 0 || index >= pages.Length) return;

        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(i == index);
        }

        currentIndex = index;
    }
}
