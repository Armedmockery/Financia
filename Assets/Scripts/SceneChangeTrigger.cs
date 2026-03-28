using UnityEngine;

public class SceneChangeTrigger : MonoBehaviour
{
    [SerializeField] private string sceneToLoad = "";
    [SerializeField] private string triggerTag = "Player";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (string.IsNullOrEmpty(sceneToLoad)) return;

        if (other.CompareTag(triggerTag))
        {
            SceneLoader.Instance.LoadGameScene(sceneToLoad);
        }
    }
}