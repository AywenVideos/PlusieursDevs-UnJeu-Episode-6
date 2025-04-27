using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }
    [SerializeField] AudioSource ambianceSource;
    [SerializeField] AudioSource sFXSource;
    [SerializeField] float ambienceTransitionTime = 1.0f;
    [SerializeField] List<AmbiantMusicAsset> ambiantMusicAssets;
    [SerializeField] List<SFXAsset> sfxAssets;
    private Dictionary<AmbiantMusic, AudioClip> ambiantMusicDictionary;
    private Dictionary<SFX, SFXAsset> sfxDictionary;
    private Coroutine ambianceTransitionCoroutine = null;

    /// <summary>
    /// Initialize the audio source and load the audio clips into dictionaries
    /// </summary>
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        ambiantMusicDictionary = new Dictionary<AmbiantMusic, AudioClip>();
        sfxDictionary = new Dictionary<SFX, SFXAsset>();
        foreach (var asset in ambiantMusicAssets)
        {
            ambiantMusicDictionary[asset.music] = asset.clip;
        }
        foreach (var asset in sfxAssets)
        {
            sfxDictionary[asset.sfx] = asset;
        }
    }

    /// <summary>
    /// Play the specified ambient music.
    /// </summary>
    /// <param name="music"> The ambient music to play.</param>
    /// <param name="smoothTransition"> If true, the transition will be smooth.</param>
    public void PlayAmbiantMusic(AmbiantMusic music, bool smoothTransition)
    {
        if (ambiantMusicDictionary.TryGetValue(music, out AudioClip clip))
        {
            if (smoothTransition)
            {
                if (ambianceTransitionCoroutine != null)
                    StopCoroutine(ambianceTransitionCoroutine);
                ambianceTransitionCoroutine = StartCoroutine(ambianteTransition(music));
            }
            else
            {
                ambianceSource.Stop();
                ambianceSource.clip = clip;
                ambianceSource.Play();
                ambianceSource.loop = true;
            }
        }
        else
        {
            Debug.LogWarning($"Ambiant music {music} not found.");
        }
    }

    /// <summary>
    /// Coroutine to handle the transition between ambient music tracks.
    /// </summary>
    /// <param name="music"> The ambient music to play.</param>
    /// <returns> IEnumerator for the coroutine.</returns>
    IEnumerator ambianteTransition(AmbiantMusic music)
    {
        // fade out current music, set new music, fade in
        while (ambianceSource.volume > 0f)
        {
            ambianceSource.volume -= Time.deltaTime / ambienceTransitionTime;
            yield return null;
        }
        ambianceSource.Stop();
        ambianceSource.clip = ambiantMusicDictionary[music];
        ambianceSource.Play();
        ambianceSource.loop = true;
        while (ambianceSource.volume < 1f)
        {
            ambianceSource.volume += Time.deltaTime / ambienceTransitionTime;
            yield return null;
        }
        ambianceSource.volume = 1f;
    }

    /// <summary>
    /// Stop the currently playing ambient music.
    /// </summary>
    public void StopAmbiantMusic()
    {
        ambianceSource.Stop();
    }

    /// <summary>
    /// Play the specified sound effect (SFX).
    /// </summary>
    /// <param name="sfx"> The sound effect to play.</param>
    public void PlaySFX(SFX sfx)
    {
        if (sfxDictionary.TryGetValue(sfx, out SFXAsset asset))
        {
            sFXSource.volume = asset.volume;
            if (asset.minPitch != asset.maxPitch)
                sFXSource.pitch = Random.Range(asset.minPitch, asset.maxPitch);
            else
                sFXSource.pitch = asset.minPitch;
            sFXSource.PlayOneShot(asset.clip);
        }
        else
        {
            Debug.LogWarning($"SFX {sfx} not found.");
        }
    }
}

[System.Serializable]
public class AmbiantMusicAsset
{
    public AmbiantMusic music;
    public AudioClip clip;
}

[System.Serializable]
public class SFXAsset
{
    public SFX sfx;
    public AudioClip clip;
    public float volume = 1.0f;
    public float minPitch = 1.0f;
    public float maxPitch = 1.0f;
}

/// <summary>
/// Enum for ambient music types.
/// </summary>
public enum AmbiantMusic
{
    None,
    Home,
    Magasin,
    City,
    Menu
}

/// <summary>
/// Enum for sound effects (SFX) types.
/// </summary>
public enum SFX
{
    None,
    Scream,
    Interract,
    Teleport,
    Block_Break,
    Block_Place,
    Menu_Opening,
    Step,
    Grab,
    Coin
}