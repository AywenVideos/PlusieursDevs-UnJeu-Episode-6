using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameConnectionController : MonoBehaviour
{
    private void Start()
    {
        NetworkManager.Initialize();
    }

    public void Connect()
    {
        // Check if the client is connected and the player is not null
        if (!NSClient.IsConnected || NetworkManager.CurrentPlayer == null)
        {
            NotificationManager.Instance.ShowNotification("You are not connected to the server or the player is null.");
            return;
        }

        // Load the game scene Asynchronously
        StartCoroutine(LoadGameScene(() =>
        {
            // Join the game's world
            NetworkManager.EnterMap(NetworkManager.CurrentPlayer.MapName, NetworkManager.CurrentPlayer.x, NetworkManager.CurrentPlayer.y, NetworkManager.CurrentPlayer.z);
        }));
    }

    IEnumerator LoadGameScene(Action callback)
    {
        // Load the game scene asynchronously
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Main");
        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Call the callback function after the scene is loaded
        callback?.Invoke();
    }
}