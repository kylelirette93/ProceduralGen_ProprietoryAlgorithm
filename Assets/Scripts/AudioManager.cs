using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Basic audio manager for playing sound effects.
/// </summary>
public class AudioManager : MonoBehaviour
{
    Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();
    [SerializeField] List<AudioClip> clips = new List<AudioClip>();
    [SerializeField] AudioSource sfxSource;

    public static AudioManager Instance;

    private void Awake()
    {
        #region Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        #endregion
    }

    private void Start()
    {
        PopulateAudioLibrary();
    }

    /// <summary>
    /// Populate audio, because dictionarys are not serializable for whatever reason.
    /// </summary>
    void PopulateAudioLibrary()
    {
        foreach (AudioClip clip in clips)
        {
            audioClips[clip.name] = clip;
        }
    }

    /// <summary>
    /// Play sound effect based on name of it.
    /// </summary>
    /// <param name="clipName">Name of the sound effect, probably a more efficient way to do this, but here we are.</param>
    public void PlaySound(string clipName)
    {
        if (audioClips.TryGetValue(clipName, out AudioClip clip))
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}
