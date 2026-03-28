using UnityEngine;
using UnityEngine.UI;

public class SoundEffectManager : MonoBehaviour
{
    public static SoundEffectManager Instance;

    public SoundEffectLibrary library;

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [SerializeField]private Slider SfxSlider;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
           
        }
        else Destroy(gameObject);
    }
    void Start(){
        SfxSlider.onValueChanged.AddListener(delegate {OnValueChanged();});
    }

    // 🎵 BGM
    public void PlayBGM(string name)
    {
        AudioClip clip = library.GetClip(name);
        if (clip == null) return;

        if (bgmSource.clip == clip) return;

        bgmSource.Stop();
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    // 🔊 Global sounds
    public void PlaySFX(string name)
    {
        AudioClip clip = library.GetRandomClip(name);
        if (clip != null)
            sfxSource.PlayOneShot(clip);
    }

    // 📍 World sounds (NPCs, Cars, etc)
    public void PlayWorldSound(string name, Vector3 position)
    {
        AudioClip clip = library.GetRandomClip(name);
        if (clip == null) return;

        AudioSource.PlayClipAtPoint(clip, position, 1f);
    }

    public void SetVolume(float volume){
        sfxSource.volume=volume;
        bgmSource.volume=volume;
    }
    public void OnValueChanged(){
        SetVolume(SfxSlider.value);
    }
}
