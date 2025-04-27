using Habbo_Common.GameEngine;
using TMPro;
using UnityEngine;

public class TopBar : MonoBehaviour
{
    public GameObject unauthenticatedUI;
    public TextMeshProUGUI usernameText;
    public TextMeshProUGUI coinsText;

    private void Start()
    {
        DisplayUnauthenticatedUI();
        // Register to the event
        EventManager.OnUserLoggedIn += HandleUserLogin;
    }

    private void DisplayAuthenticatedUI()
    {
        unauthenticatedUI?.SetActive(false);

        usernameText.text = NetworkManager.CurrentPlayer.Name;
        coinsText.text = NetworkManager.CurrentPlayer.Money.ToString();
    }

    private void DisplayUnauthenticatedUI()
    {
        unauthenticatedUI?.SetActive(true);
    }

    private void HandleUserLogin(Player player)
    {
        DisplayAuthenticatedUI();
    }

    // Assurez-vous de vous désabonner des événements lors de la destruction de l'objet
    private void OnDestroy()
    {
        EventManager.OnUserLoggedIn -= HandleUserLogin;
    }
}
