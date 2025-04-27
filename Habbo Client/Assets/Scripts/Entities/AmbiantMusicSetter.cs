using UnityEngine;

public class AmbiantMusicSetter : MonoBehaviour
{
    public AmbiantMusic Music;
    public bool SmoothTransition = true;

    void Start()
    {
        MusicManager.Instance.PlayAmbiantMusic(Music, SmoothTransition);
        Destroy(this);
    }
}