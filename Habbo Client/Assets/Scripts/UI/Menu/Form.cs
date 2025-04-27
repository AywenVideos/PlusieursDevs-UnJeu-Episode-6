using Habbo_Common;
using Habbo_Common.GameEngine;
using NetSquare.Core;
using TMPro;
using UnityEngine;

public class Form : MonoBehaviour
{
    public TextMeshProUGUI usernameText;
    public TextMeshProUGUI passwordText;

    /// <summary>
    /// Called when the login button is clicked.
    /// </summary>
    public void OnLoginButtonClick()
    {
        string username = usernameText.text;
        string password = passwordText.text;

        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
        {
            NSClient.SendMessage(new NetworkMessage(MessageTypes.ToServer_Login).Set(username).Set(password), (reply) =>
            {
                if (reply.Serializer.GetBool())
                {
                    Debug.Log("Connexion réussie !");
                    // Get the player data from the server
                    Player player = new Player(reply.Serializer);
                    NetworkManager.CurrentPlayer = player;
                    // Trigger the user login event
                    EventManager.TriggerUserLogin(player);
                }
                else
                {
                    NotificationManager.Instance.ShowNotification("Échec de la connexion : " + reply.Serializer.GetString());
                }
            });
        }
        else
        {
            NotificationManager.Instance.ShowNotification("Veuillez entrer un nom d'utilisateur et un mot de passe.");
        }
    }

    /// <summary>
    /// Called when the register button is clicked.
    /// </summary>
    public void OnRegisterButtonClick()
    {
        NSClient.SendMessage(new NetworkMessage(MessageTypes.ToServer_Register).Set(usernameText.text).Set(passwordText.text), (reply) => { 
            if(reply.Serializer.GetBool())
            {
                NotificationManager.Instance.ShowNotification("Inscription réussie !");
            }
            else
            {
                NotificationManager.Instance.ShowNotification("Échec de l'inscription : " + reply.Serializer.GetString());
            }
        });
    }
}