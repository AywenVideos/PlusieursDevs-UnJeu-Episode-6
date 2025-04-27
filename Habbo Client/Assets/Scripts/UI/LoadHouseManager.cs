using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadHouseManager : MonoBehaviour
{
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(LoadScene);
    }

    public void LoadScene()
    {
        SceneManager.LoadScene("Assets/Scenes/MainScene.unity");
    }
}
