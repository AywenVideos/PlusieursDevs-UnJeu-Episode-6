using UnityEngine;
using UnityEngine.UI;

public class TestTextUpdate : MonoBehaviour
{
    public Text testText;

    void Start()
    {
        if (testText != null)
        {
            testText.text = "Test de mise à jour du texte réussi !";
        }
        else
        {
            Debug.LogError("testText est nul !");
        }
    }
} 