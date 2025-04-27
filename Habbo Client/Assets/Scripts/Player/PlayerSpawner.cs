using Habbo_Common.GameEngine;
using NetSquare.Core;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public static PlayerSpawner Instance { get; private set; }
    public GameObject LocalPlayerPrefab;
    public GameObject RemotePlayerPrefab;

    /// <summary>
    /// Called when the object is instantiated.
    /// </summary>
    public void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Subscribe to events
        NSClient.Client.WorldsManager.OnClientJoinWorld += WorldsManager_OnClientJoinWorld;
        NSClient.Client.WorldsManager.OnClientLeaveWorld += WorldsManager_OnClientLeaveWorld;
    }

    /// <summary>
    /// Called when a player joins the world.
    /// </summary>
    /// <param name="clientID"> The ID of the client that joined the world.</param>
    /// <param name="frame"> The transform frame of the player.</param>
    /// <param name="message"> The network message containing player data.</param>
    private void WorldsManager_OnClientJoinWorld(uint clientID, NetsquareTransformFrame frame, NetworkMessage message)
    {
        Player player = new Player(message.Serializer);
        if (player == null)
        {
            Debug.LogError("Player is null");
            return;
        }

        // Check if the player is the local player
        if (clientID == NSClient.ClientID)
        {
            SpawnLocalPlayer(player);
        }
        else
        {
            SpawnRemotePlayer(clientID, player);
        }
    }

    /// <summary>
    /// Called when a player leaves the world.
    /// </summary>
    /// <param name="clientID"> The ID of the client that left the world.</param>
    private void WorldsManager_OnClientLeaveWorld(uint clientID)
    {
        if (!NetworkManager.Players.ContainsKey(clientID))
        {
            Debug.LogError("Player not found in dictionary");
            return;
        }

        // Destroy the player object
        if (NetworkManager.Players.TryGetValue(clientID, out GameObject playerObject))
        {
            Destroy(playerObject);
            NetworkManager.Players.Remove(clientID);
        }
    }

    /// <summary>
    /// Spawns the local player in the world.
    /// </summary>
    /// <param name="player"> The player object.</param>
    private void SpawnLocalPlayer(Player player)
    {
        if(NetworkManager.CurrentPlayerController != null)
        {
            Destroy(NetworkManager.CurrentPlayerController.gameObject);
            NetworkManager.CurrentPlayerController = null;
        }
        GameObject localPlayer = Instantiate(LocalPlayerPrefab);
        localPlayer.GetComponent<PlayerController>().Initialize(player);
        NetworkManager.CurrentPlayer = player;
        NetworkManager.CurrentPlayerController = localPlayer.GetComponent<PlayerController>();
    }

    /// <summary>
    /// Spawns a remote player in the world.
    /// </summary>
    /// <param name="clientID"> The ID of the client.</param>
    /// <param name="player"> The player object.</param>
    private void SpawnRemotePlayer(uint clientID, Player player)
    {
        GameObject remotePlayer = Instantiate(RemotePlayerPrefab);
        remotePlayer.GetComponent<RemotePlayer>().Initialize(player);
        NetworkManager.Players.Add(clientID, remotePlayer);
    }

    /// <summary>
    /// Called when the object is destroyed.
    /// </summary>
    private void OnDestroy()
    {
        // Unsubscribe from events
        NSClient.Client.WorldsManager.OnClientJoinWorld -= WorldsManager_OnClientJoinWorld;
        NSClient.Client.WorldsManager.OnClientLeaveWorld -= WorldsManager_OnClientLeaveWorld;
    }
}