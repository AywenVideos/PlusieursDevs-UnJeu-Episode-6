using UnityEngine.Video;

public class TVWindow : WindowController
{
    public static TVWindow Instance;
    public VideoPlayer videoPlayer;

    public void Awake()
    {
        Instance = this;
    }

    public override void OpenWindow()
    {
        base.OpenWindow();
        videoPlayer.Play();
    }

    public override void CloseWindow()
    {
        base.CloseWindow();
        videoPlayer.Pause();
    }
}