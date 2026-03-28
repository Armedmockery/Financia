using System.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader instance;
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] float fadeDuration = 0.5f;
    [SerializeField] CinemachineCamera vcam;

    CinemachinePositionComposer transposer;
    Vector3 originalDamping;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        transposer = vcam.GetComponent<CinemachinePositionComposer>();
        if (transposer != null)
        {
            originalDamping = transposer.Damping;
        }
    }

    async Task Fade(float targetTransparency)
    {
        float start = canvasGroup.alpha, t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, targetTransparency, t / fadeDuration);
            await Task.Yield();
        }
        canvasGroup.alpha = targetTransparency;
    }

    public async Task FadeOut()
    {
        await Fade(1);
        SetDamping(Vector3.zero);
    }

    public async Task FadeIn()
    {
        await Fade(0);
        SetDamping(originalDamping);
    }

    void SetDamping(Vector3 d)
    {
        if (transposer == null) return;
        transposer.Damping = d;
    }
}
