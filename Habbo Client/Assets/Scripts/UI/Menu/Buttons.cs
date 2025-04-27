using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NewMonoBehaviourScript : MonoBehaviour
{

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(LoadScene);
    }

    void LoadScene()
    {
        if (gameObject.name == "Jouer")
        {
            SceneManager.LoadScene("Assets/Scenes/MainScene.unity");
        }
        else if (gameObject.name == "Options/Credits")
        {

        }
        else if (gameObject.name == "Quitter")
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        else if (gameObject.name == "Quit")
        {
            SceneManager.LoadScene("Assets/Scenes/Menu.unity");
        }

        else if (gameObject.name == "BackToHouse")
        {
            SceneManager.LoadScene("Assets/Scenes/MainScene.unity");
        }
    }
}
