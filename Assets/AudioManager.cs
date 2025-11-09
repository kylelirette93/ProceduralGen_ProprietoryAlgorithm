using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();
    [SerializeField] List<AudioClip> clips = new List<AudioClip>();
    public AudioSource sfxSource;

    public static AudioManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PopulateAudioLibrary();
    }

    void PopulateAudioLibrary()
    {
        foreach (AudioClip clip in clips)
        {
            audioClips[clip.name] = clip;
        }
    }

    public void PlaySound(string clipName)
    {
        if (audioClips.TryGetValue(clipName, out AudioClip clip))
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}
