using System.Collections.Generic;
using UnityEngine;

public class SoundEffectLibrary : MonoBehaviour
{
    [SerializeField]
    private SoundEffectGroup[] soundEffectGroups;

    private Dictionary<string, List<AudioClip>> soundDictionary;

    private void Awake()
    {
        InitializeDictionary();
    }

    private void InitializeDictionary()
    {
        soundDictionary = new Dictionary<string, List<AudioClip>>();

        foreach (SoundEffectGroup soundEffectGroup in soundEffectGroups)
        {
            if (!soundDictionary.ContainsKey(soundEffectGroup.name))
            {
                soundDictionary.Add(
                    soundEffectGroup.name,
                    soundEffectGroup.audioClips
                );
            }
        }
    }

    public AudioClip GetRandomClip(string name)
    {
        if (soundDictionary.ContainsKey(name))
        {
            List<AudioClip> audioClips = soundDictionary[name];

            if (audioClips != null && audioClips.Count > 0)
            {
                return audioClips[Random.Range(0, audioClips.Count)];
            }
        }

        Debug.LogWarning($"SoundEffectLibrary: No sound found for key '{name}'");
        return null;
    }
    public AudioClip GetClip(string name)
    {
        if (soundDictionary.ContainsKey(name))
        {
            return soundDictionary[name][0];
        }

        Debug.LogWarning($"SoundEffectLibrary: No sound found for key '{name}'");
        return null;
    }
}

[System.Serializable]
public struct SoundEffectGroup
{
    public string name;
    public List<AudioClip> audioClips;
}
