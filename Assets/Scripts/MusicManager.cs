using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private string startMusic = "Peaceful";

    private AudioSource musicSource;
    private SoundEffectLibrary library;
    private string currentMusic;

    private void Awake()
    {
        // second AudioSource = music
        AudioSource[] sources = GetComponents<AudioSource>();
        if (sources.Length < 2)
        {
            Debug.LogError("MusicManager needs TWO AudioSources!");
            return;
        }

        musicSource = sources[1];
        library = GetComponent<SoundEffectLibrary>();
    }

    private void Start()
    {
        PlayMusic(startMusic);
    }

    public void PlayMusic(string name)
    {
        if (currentMusic == name) return;

        AudioClip clip = library.GetRandomClip(name);
        if (clip == null) return;

        currentMusic = name;
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
        currentMusic = null;
    }
}
