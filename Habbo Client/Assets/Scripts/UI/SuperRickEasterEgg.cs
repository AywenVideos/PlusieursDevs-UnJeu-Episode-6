using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;

public class OpenYouTubeLink : MonoBehaviour
{
    private string youtubeUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";

    void Start()
    {
        Button button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.AddListener(OpenLink);
        }
    }

    public void OpenLink()
    {
        Application.OpenURL(youtubeUrl);
        print("Rick activated!");
    }
}
