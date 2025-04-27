using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class PlayVideoManager : MonoBehaviour
{
    public GameObject[] objectsToDisable;
    public GameObject[] objectsToEnable;
    public Behaviour[] behavioursToDisable;
    public VideoPlayer player;
    public Image windowImage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DisableForVideo()
    {
        for (int i = 0; i < objectsToDisable.Length; i++)
        {
            objectsToDisable[i].SetActive(false);
        }
        for (int i = 0; i < objectsToEnable.Length; i++)
        {
            objectsToEnable[i].SetActive(true);
        }
        for (int i = 0; i < behavioursToDisable.Length; i++)
        {
            behavioursToDisable[i].enabled = false;
        }
        windowImage.enabled = false;
    }

    public void EnableAfterVideo()
    {
        for (int i = 0; i < objectsToDisable.Length; i++)
        {
            objectsToDisable[i].SetActive(true);
        }
        for (int i = 0; i < objectsToEnable.Length; i++)
        {
            objectsToEnable[i].SetActive(false);
        }
        for (int i = 0; i < behavioursToDisable.Length; i++)
        {
            behavioursToDisable[i].enabled = true;
        }
        windowImage.enabled = true;
    }

    void OnMovieFinished(VideoPlayer _player)
    {
        _player = player;
        EnableAfterVideo();
    }
}
