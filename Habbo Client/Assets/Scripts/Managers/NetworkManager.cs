using Habbo_Common;
using Habbo_Common.GameEngine;
using NetSquare.Core;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public static class NetworkManager
{
    public static Dictionary<uint, GameObject> Players = new Dictionary<uint, GameObject>();
    public static Player CurrentPlayer;
    public static PlayerController CurrentPlayerController;
    public static bool IsInMap = false;

    public static void Initialize()
    {
        NSClient.OnConnected += NSClient_OnConnected;
    }

    /// <summary>
    /// Called when the client is connected to the server.
    /// </summary>
    /// <param name="clientID"> The ID of the client that connected.</param>
    private static void NSClient_OnConnected(uint clientID)
    {
        // Request the player data from the server
        NSClient.SendMessage(new NetworkMessage(MessageTypes.ToServer_GetPlayer), (NetworkMessage) =>
        {
            // Deserialize the player data
            CurrentPlayer = new Player(NetworkMessage.Serializer);
        });
    }

    #region Players
    /// <summary>
    /// Enter a map
    /// </summary>
    /// <param name="mapName"> Map name </param>
    /// <param name="x"> X position </param>
    /// <param name="y"> <param name="y"> Y position </param>
    public static void EnterMap(MapNames mapName, float x, float y, float z)
    {
        if (IsInMap)
        {
            NSClient.Client.WorldsManager.TryleaveWorld((leaved) =>
            {
                if (leaved)
                {
                    // Unspawn all players
                    foreach (var player in Players)
                    {
                        GameObject.Destroy(player.Value);
                    }
                    Players.Clear();

                    // Unspawn the current player
                    if (CurrentPlayerController != null)
                    {
                        GameObject.Destroy(CurrentPlayerController.gameObject);
                        CurrentPlayerController = null;
                    }

                    // clear leaved map
                    NSClient.Client.WorldsManager.TryJoinWorld((ushort)mapName, new NetsquareTransformFrame(x, y, z), (success) =>
                    {
                        if (success)
                        {
                            IsInMap = true;
                            CurrentPlayer.MapName = mapName;
                            MapManager.Instance.LoadMap(mapName);
                        }
                        else
                        {
                            Debug.Log("can't join world");
                        }
                    });
                }
                else
                {
                    Debug.Log("can't leave world");
                }
            });
        }
        else
        {
            NSClient.Client.WorldsManager.TryJoinWorld((ushort)mapName, new NetsquareTransformFrame(x, y, z), (success) =>
            {
                if (success)
                {
                    IsInMap = true;
                    CurrentPlayer.MapName = mapName;
                    MapManager.Instance.LoadMap(mapName);
                }
                else
                {
                    Debug.Log("can't join world");
                }
            });
        }
    }

    /// <summary>
    /// Get player data from the server
    /// </summary>
    /// <param name="playerID"> The ID of the player to get data for.</param>
    /// <param name="callback"> The callback to execute with the player data.</param>
    public static void GetPlayer(uint playerID, Action<Player> callback)
    {
        // request the player data from the server
        NSClient.SendMessage(new NetworkMessage(MessageTypes.ToServer_GetPlayer), (reply) =>
        {
            if (reply.Serializer.GetBool())
            {
                // Deserialize the player data
                Player player = new Player(reply.Serializer);
                callback?.Invoke(player);
            }
            else
            {
                Debug.LogError(reply.Serializer.GetString());
                callback?.Invoke(null);
            }
        });
    }
    #endregion

    #region Locomotion
    /// <summary>
    /// Ask the server to move the player to a specific position.
    /// </summary>
    /// <param name="targetPos"> The position to move to.</param>
    public static void AskToMove(Vector3 targetPos)
    {
        // Get the current cell
        if (CurrentPlayerController == null)
        {
            return;
        }

        // Get the current cell
        Cell currentCell = MapManager.Instance.CurrentMap.GetCell(CurrentPlayerController.transform.position - MapManager.Instance.CurrentMap.PlayerOffset);
        // check if the cell is walkable
        if (currentCell == null || !currentCell.CanWalkOn)
        {
            return;
        }

        CurrentPlayerController.CurrentCell = currentCell;
        // Get the destination cell
        Cell destinationCell = MapManager.Instance.CurrentMap.GetCell(targetPos);
        // check if the destination cell is walkable
        if (destinationCell == null || !destinationCell.CanWalkOn)
        {
            return;
        }

        // Get the path between the current cell and the destination cell
        Stack<Cell> path = MapManager.Instance.CurrentMap.FindPath(currentCell, destinationCell);
        if (path == null || path.Count == 0)
        {
            return;
        }

        // Ask the server to move the player
        NSClient.SendMessage(new NetworkMessage(MessageTypes.ToServer_PlayerMove)
            .Set(CurrentPlayer.ID).Set(targetPos.x).Set(targetPos.y).Set(targetPos.z));
    }

    /// <summary>
    /// Handle the player movement message from the server.
    /// </summary>
    /// <param name="message"> The message from the server.</param>
    [NetSquareAction((ushort)MessageTypes.ToClient_PlayerMove)]
    public static void PlayerMove(NetworkMessage message)
    {
        // Get the player ID
        uint playerID = message.Serializer.GetUInt();
        // Get the player position
        Vector3 position = new Vector3(message.Serializer.GetFloat(), message.Serializer.GetFloat(), message.Serializer.GetFloat());

        // handle local player
        if (playerID == CurrentPlayer.ID)
        {
            // Set the player position
            CurrentPlayerController.GoToPosition(position);
        }
        // handle remote player
        else
        {
            // check if the player is already spawned
            if (Players.ContainsKey(message.ClientID))
            {
                // Get the player object
                GameObject playerObject = Players[message.ClientID];
                // Get the player controller
                RemotePlayer playerController = playerObject.GetComponent<RemotePlayer>();
                // Set the player position
                playerController.GoToPosition(position);
            }
        }
    }

    /// <summary>
    /// Ask the server to teleport the player to a specific position.
    /// </summary>
    /// <param name="player"> The player to teleport.</param>
    /// <param name="targetPos"> The position to teleport to.</param>
    public static void AskToTeleport(Player player, Vector3 targetPos)
    {
        // Get the destination cell
        Cell destinationCell = MapManager.Instance.CurrentMap.GetCell(targetPos);
        // check if the destination cell is walkable
        if (destinationCell == null || !destinationCell.CanWalkOn)
        {
            return;
        }
        // Ask the server to move the player
        NSClient.SendMessage(new NetworkMessage(MessageTypes.ToServer_Teleport)
            .Set(player.ID).Set(targetPos.x).Set(targetPos.y).Set(targetPos.z));
    }

    /// <summary>
    /// Handle the player teleport message from the server.
    /// </summary>
    /// <param name="message"> The message from the server.</param>
    [NetSquareAction((ushort)MessageTypes.ToClient_Teleport)]
    public static void PlayerTeleport(NetworkMessage message)
    {
        // Get the player ID
        uint playerID = message.Serializer.GetUInt();
        // Get the player position
        Vector3 position = new Vector3(message.Serializer.GetFloat(), message.Serializer.GetFloat(), message.Serializer.GetFloat());
        // handle local player
        if (playerID == CurrentPlayer.ID)
        {
            // Set the player position
            CurrentPlayerController.transform.position = position;
            // Set the player cell
            CurrentPlayerController.CurrentCell = MapManager.Instance.CurrentMap.GetCell(position);
        }
        // handle remote player
        else
        {
            // check if the player is already spawned
            if (Players.ContainsKey(message.ClientID))
            {
                // Get the player object
                GameObject playerObject = Players[message.ClientID];
                // Get the player controller
                RemotePlayer playerController = playerObject.GetComponent<RemotePlayer>();
                // Set the player position
                playerController.transform.position = position;
                // Set the player cell
                playerController.CurrentCell = MapManager.Instance.CurrentMap.GetCell(position);
            }
        }
    }
    #endregion

    #region Skins
    /// <summary>
    /// Set the player skin.
    /// </summary>
    /// <param name="colorIndex"> The index of the skin color. </param>
    public static void PlayerSetColor(int colorIndex)
    {
        // Ask the server to set the player color
        NSClient.SendMessage(new NetworkMessage(MessageTypes.ToServer_PlayerSetColor)
            .Set(CurrentPlayer.ID).Set(colorIndex));
    }

    [NetSquareAction((ushort)MessageTypes.ToClient_PlayerSetColor)]
    public static void PlayerSetColor(NetworkMessage message)
    {
        // Get the player ID
        uint playerID = message.Serializer.GetUInt();
        // Get the player color
        int colorIndex = message.Serializer.GetInt();
        // handle local player
        if (playerID == CurrentPlayer.ID)
        {
            // Set the player color
            CurrentPlayer.Color = colorIndex;
            PlayerSkin.Instance.SetColor(CurrentPlayerController.PlayerSpriteRenderer.material, colorIndex);
        }
        // handle remote player
        else
        {
            // check if the player is already spawned
            if (Players.ContainsKey(message.ClientID))
            {
                // Get the player object
                GameObject playerObject = Players[message.ClientID];
                // Get the player controller
                RemotePlayer playerController = playerObject.GetComponent<RemotePlayer>();
                // Set the player color
                PlayerSkin.Instance.SetColor(playerController.PlayerSpriteRenderer.material, colorIndex);
            }
        }
    }
    #endregion

    #region Chat
    /// <summary>
    /// Send a chat message to the server.
    /// </summary>
    /// <param name="message"> The chat message to send.</param>
    public static void Chat(string message)
    {
        // Ask the server to send the chat message
        NSClient.SendMessage(new NetworkMessage(MessageTypes.ToServer_Chat)
            .Set(CurrentPlayer.ID).Set(message));
    }

    /// <summary>
    /// Handle the chat message from the server.
    /// </summary>
    /// <param name="message"> The message from the server.</param>
    [NetSquareAction((ushort)MessageTypes.ToClient_Chat)]
    public static void Chat(NetworkMessage message)
    {
        // Get the player ID
        uint playerID = message.Serializer.GetUInt();
        // Get the chat message
        string chatMessage = message.Serializer.GetString();

        Vector3 playerPos = Vector3.zero;
        string playerName = string.Empty;
        // handle local player
        if (playerID == CurrentPlayer.ID)
        {
            // Set the player position
            playerPos = CurrentPlayerController.transform.position;
            playerName = CurrentPlayerController.Player.Name;
        }
        // handle remote player
        else
        {
            // check if the player is already spawned
            if (Players.ContainsKey(message.ClientID))
            {
                // Get the player object
                GameObject playerObject = Players[message.ClientID];
                // Set the player position
                playerPos = playerObject.transform.position;
                playerName = playerObject.GetComponent<RemotePlayer>().Player.Name;
            }
        }

        PlayerChat.Instance.WriteMessage(playerName, chatMessage, playerPos);
    }
    #endregion

    #region Name
    /// <summary>
    /// Ask the server to update the player name.
    /// </summary>
    /// <param name="name"> The new player name.</param>
    public static void AskUpdatePlayerName(string name)
    {
        // Ask the server to update the player name
        NSClient.SendMessage(new NetworkMessage(MessageTypes.ToServer_Rename)
            .Set(CurrentPlayer.ID).Set(name));
    }

    /// <summary>
    /// Handle the player rename message from the server.
    /// </summary>
    /// <param name="message"> The message from the server.</param>
    [NetSquareAction((ushort)MessageTypes.ToClient_Rename)]
    public static void PlayerRename(NetworkMessage message)
    {
        // Get the player ID
        uint playerID = message.Serializer.GetUInt();
        // Get the player name
        string name = message.Serializer.GetString();
        // handle local player
        if (playerID == CurrentPlayer.ID)
        {
            // Set the player name
            CurrentPlayer.Name = name;
            CurrentPlayerController.Player.Name = name;
        }
        // handle remote player
        else
        {
            // check if the player is already spawned
            if (Players.ContainsKey(message.ClientID))
            {
                // Get the player object
                GameObject playerObject = Players[message.ClientID];
                // Get the player controller
                RemotePlayer playerController = playerObject.GetComponent<RemotePlayer>();
                playerController.Player.Name = name;
            }
        }
    }
    #endregion

    #region Building
    /// <summary>
    /// Ask the server to place a block at a specific cell.
    /// </summary>
    /// <param name="cell"> The cell to place the block at.</param>
    /// <param name="blockId"> The ID of the block to place.</param>
    public static void AskToPlaceBlock(Cell cell, int blockId)
    {
        NSClient.SendMessage(new NetworkMessage(MessageTypes.ToServer_PoseBlock)
            .Set(cell.X).Set(cell.Y).Set(blockId));
    }

    /// <summary>
    /// Handle the block placement message from the server.
    /// </summary>
    /// <param name="cell"> The cell to place the block at.</param>
    public static void AskToRemoveBlock(Cell cell)
    {
        NSClient.SendMessage(new NetworkMessage(MessageTypes.ToServer_BreakBlock)
            .Set(cell.X).Set(cell.Y));
    }

    /// <summary>
    /// Handle the block placement message from the server.
    /// </summary>
    /// <param name="message"> The message from the server.</param>
    [NetSquareAction((ushort)MessageTypes.ToClient_SendBlocks)]
    public static void PlaceBlock(NetworkMessage message)
    {
        Blocks blocks = new Blocks(message.Serializer);
        PlayerBuild.Instance.SetBlocks(blocks);
    }

    /// <summary>
    /// Get the blocks from the server.
    /// </summary>
    public static void GetBlocks()
    {
        NSClient.SendMessage(new NetworkMessage(MessageTypes.ToServer_GetBlocks), (message) =>
        {
            Blocks blocks = new Blocks(message.Serializer);
            PlayerBuild.Instance.SetBlocks(blocks);
        });
    }
    #endregion

    #region Gold
    /// <summary>
    /// Add gold to the player.
    /// </summary>
    /// <param name="gold"> The amount of gold to add.</param>
    public static void AddGold(int gold)
    {
        NSClient.SendMessage(new NetworkMessage(MessageTypes.ToServer_AddGold)
            .Set(CurrentPlayer.ID).Set(gold));
    }

    /// <summary>
    /// Handle the add gold message from the server.
    /// </summary>
    /// <param name="message"> The message from the server.</param>
    [NetSquareAction((ushort)MessageTypes.ToClient_AddGold)]
    public static void AddGold(NetworkMessage message)
    {
        // Get the player ID
        uint playerID = message.Serializer.GetUInt();
        // Get the player gold
        int gold = message.Serializer.GetInt();
        // handle local player
        if (playerID == CurrentPlayer.ID)
        {
            // Set the player gold
            CurrentPlayer.Money = gold;
        }

        EventManager.TriggerUpdateMoney(CurrentPlayer.Money);
    }
    #endregion
}