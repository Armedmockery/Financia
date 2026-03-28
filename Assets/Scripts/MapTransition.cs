using Unity.Cinemachine;
using UnityEngine;


public class MapTransition : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] PolygonCollider2D mapBoundry;
    CinemachineConfiner2D confiner;
    [SerializeField] Direction direction;
    [SerializeField] private MapArea mapArea;
    [SerializeField] Transform teleportTargetPosition;

    public string musicName;
    enum Direction { Up,Down , Left, Right,Teleport }
    private void Awake()
    {
        confiner = FindAnyObjectByType<CinemachineConfiner2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            FadeTransition(collision.gameObject);
            MapController_Manual.instance?.HighlightArea(mapBoundry.name);
            SoundEffectManager.Instance.PlayBGM(musicName);
        }
    }

    async void FadeTransition(GameObject player)
    {
        await ScreenFader.instance.FadeOut();
        confiner.BoundingShape2D = mapBoundry;
        UpdatePlayerPosition(player);
        await ScreenFader.instance.FadeIn();

    }

    private void UpdatePlayerPosition(GameObject player)
    {
        Vector3 newPos = player.transform.position;

        if (direction == Direction.Teleport)
        {
            player.transform.position = teleportTargetPosition.position;
            return;
        }

        switch (direction)
        {

            case Direction.Up:
                newPos.y += 2;
                break;
            case Direction.Down:
                newPos.y -= 2;
                break;
            case Direction.Left:
                newPos.x += 2;
                break;
            case Direction.Right:
                newPos.x -= 2;
                break;
        }

        player.transform.position = newPos;

    }
        
}
