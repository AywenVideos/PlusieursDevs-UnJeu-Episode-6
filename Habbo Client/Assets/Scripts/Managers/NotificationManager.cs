using Habbo_Common.GameEngine;
using TMPro;
using UnityEngine;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance;
    public TextMeshProUGUI messageText;  // Référence à votre TextMeshProUGUI pour afficher le message
    public WindowController window; // Référence au panneau de notification qui contiendra le texte

    private void Awake()
    {
        // Check if an instance of NotificationManager already exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Abonnement aux événements du EventManager
        EventManager.OnUserLoggedIn += ShowLoginSuccessMessage;
        EventManager.OnUserLoggedOut += ShowLogoutMessage;
        EventManager.OnUserRegistered += ShowRegistrationSuccessMessage;
        EventManager.OnUserRegistrationError += ShowRegistrationErrorMessage;
        EventManager.OnErrorOccurred += ShowGeneralErrorMessage;

        // Cache la notification au démarrage
        HideNotification();
    }

    private void Update()
    {
        // toggle fullscreen
        if (Input.GetKeyDown(KeyCode.F11))
        {
            if (Screen.fullScreen)
            {
                Screen.fullScreen = false;
            }
            else
            {
                Screen.fullScreen = true;

                // Set the screen resolution to current resolution
                Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
            }
        }
    }

    // Méthode pour afficher un message de connexion réussie
    private void ShowLoginSuccessMessage(Player player)
    {
        ShowNotification("Connexion réussie !");
    }

    // Méthode pour afficher un message de déconnexion
    private void ShowLogoutMessage()
    {
        ShowNotification("Déconnexion réussie !");
    }

    // Méthode pour afficher un message d'inscription réussie
    private void ShowRegistrationSuccessMessage()
    {
        ShowNotification("Inscription réussie !");
    }

    // Méthode pour afficher un message d'erreur d'inscription
    private void ShowRegistrationErrorMessage(string errorMessage)
    {
        ShowNotification("Erreur inscription: " + errorMessage);
    }

    // Méthode pour afficher un message d'erreur générique
    private void ShowGeneralErrorMessage(string errorMessage)
    {
        ShowNotification("Erreur: " + errorMessage);
    }

    // Méthode pour afficher la notification avec un message
    public void ShowNotification(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
            window.OpenWindowcredit();
        }
    }

    // Méthode pour cacher la notification
    private void HideNotification()
    {
        window.CloseWindow();
    }
}
