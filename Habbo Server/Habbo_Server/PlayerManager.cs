using Habbo_Common;
using Habbo_Common.GameEngine;
using Habbo_Server.Datas;
using NetSquare.Core;
using NetSquare.Server.Utils;
using System;
using System.Linq;

namespace Habbo_Server
{
    public static class PlayerManager
    {
        /// <summary>
        /// Register a new player.
        /// </summary>
        /// <param name="message"> The network message containing the registration data.</param>
        [NetSquareAction((ushort)MessageTypes.ToServer_Register)]
        public static void Register(NetworkMessage message)
        {
            string account = message.Serializer.GetString();
            string password = message.Serializer.GetString();

            // Check if the username already exists
            if (Database.Players.Any(p => p.Value.Account == account))
            {
                message.Reply(new NetworkMessage().Set(false).Set("Account already exists."));
                return;
            }

            // Create a new player object
            Player newPlayer = new Player("Bob", 0, 0, 0, 0, 0, MapNames.Home, 0)
            {
                Account = account,
                Password = password
            };

            // Assign a unique ID to the player
            newPlayer.ID = (uint)Database.Players.Count + 1;
            Database.Players.Add(newPlayer.ID, newPlayer);
            message.Reply(new NetworkMessage().Set(true));

            // Save Database
            Database.Save();
        }

        /// <summary>
        /// Login a player to the server.
        /// </summary>
        /// <param name="message"> The network message containing the login data.</param>
        [NetSquareAction((ushort)MessageTypes.ToServer_Login)]
        public static void Login(NetworkMessage message)
        {
            string account = message.Serializer.GetString();
            string password = message.Serializer.GetString();

            // Check if the account exists
            Player player = Database.Players.Values.FirstOrDefault(p => p.Account == account && p.Password == password);
            if (player == null)
            {
                message.Reply(new NetworkMessage().Set(false).Set("Invalid account or password."));
                return;
            }

            // Check if the player is already logged in
            if(player.PlayerState != PlayerState.Disconnected)
            {
                message.Reply(new NetworkMessage().Set(false).Set("Player already logged in."));
                return;
            }

            // Update player state to logged in
            player.PlayerState = PlayerState.Connected;

            // set client ID
            player.ClientID = message.ClientID;

            // Send success message
            NetworkMessage reply = new NetworkMessage().Set(true);
            player.Serialize(reply.Serializer);
            message.Reply(reply);
        }

        /// <summary>
        /// Get player data from the server.
        /// </summary>
        /// <param name="message"> The network message containing the player ID.</param>
        [NetSquareAction((ushort)MessageTypes.ToServer_GetPlayer)]
        public static void GetPlayer(NetworkMessage message)
        {
            uint playerID = message.Serializer.GetUInt();
            Player player = GetPlayer(playerID);
            if (player == null)
            {
                message.Reply(new NetworkMessage().Set(false).Set("Player not found."));
                return;
            }

            // reply with player data
            NetworkMessage reply = new NetworkMessage().Set(true);
            player.Serialize(reply.Serializer);
            message.Reply(reply);
        }

        /// <summary>
        /// Handle player movement.
        /// </summary>
        /// <param name="message"> The network message containing the movement data.</param>
        [NetSquareAction((ushort)MessageTypes.ToServer_PlayerMove)]
        public static void PlayerMove(NetworkMessage message)
        {
            // get player ID and movement data from the message
            uint playerID = message.Serializer.GetUInt();
            float x = message.Serializer.GetFloat();
            float y = message.Serializer.GetFloat();
            float z = message.Serializer.GetFloat();

            // get player object
            Player player = GetPlayer(playerID);
            if (player == null)
            {
                Writer.Write_Server("Player not found", ConsoleColor.Red);
                return;
            }

            // update player position
            player.x = x;
            player.y = y;
            player.z = z;

            // send updated player data to all clients in the same world
            NetworkMessage reply = new NetworkMessage(MessageTypes.ToClient_PlayerMove, message.ClientID)
                .Set(playerID).Set(x).Set(y).Set(z);
            HabboServer.Server.Worlds.BroadcastToWorld(reply, false);
        }

        /// <summary>
        /// Handle player teleportation.
        /// </summary>
        /// <param name="message"> The network message containing the teleportation data.</param>
        [NetSquareAction((ushort)MessageTypes.ToServer_Teleport)]
        public static void PlayerTeleport(NetworkMessage message)
        {
            uint playerID = message.Serializer.GetUInt();
            float x = message.Serializer.GetFloat();
            float y = message.Serializer.GetFloat();
            float z = message.Serializer.GetFloat();

            // get player object
            Player player = GetPlayer(playerID);
            if (player == null)
            {
                Writer.Write_Server("Player not found", ConsoleColor.Red);
                return;
            }

            // update player position
            player.x = x;
            player.y = y;
            player.z = z;

            // send updated player data to all clients in the same world
            NetworkMessage reply = new NetworkMessage(MessageTypes.ToClient_Teleport, message.ClientID)
                .Set(playerID).Set(x).Set(y).Set(z);
            HabboServer.Server.Worlds.BroadcastToWorld(reply, false);
        }

        /// <summary>
        /// Handle player skin change.
        /// </summary>
        /// <param name="message"> The network message containing the skin change data.</param>
        [NetSquareAction((ushort)MessageTypes.ToServer_PlayerSetColor)]
        public static void PlayerSetColor(NetworkMessage message)
        {
            uint playerID = message.Serializer.GetUInt();
            int color = message.Serializer.GetInt();

            // get player object
            Player player = GetPlayer(playerID);
            if (player == null)
            {
                Writer.Write_Server("Player not found", ConsoleColor.Red);
                return;
            }

            // update player color
            player.Color = color;

            // send updated player data to all clients in the same world
            NetworkMessage reply = new NetworkMessage(MessageTypes.ToClient_PlayerSetColor, message.ClientID)
                .Set(playerID).Set(color);
            HabboServer.Server.Worlds.BroadcastToWorld(reply, false);
        }

        /// <summary>
        /// Handle player skin change.
        /// </summary>
        /// <param name="message"> The network message containing the skin change data.</param>
        [NetSquareAction((ushort)MessageTypes.ToServer_Chat)]
        public static void PlayerChat(NetworkMessage message)
        {
            uint playerID = message.Serializer.GetUInt();
            string text = message.Serializer.GetString();
            // get player object
            Player player = GetPlayer(playerID);
            if (player == null)
            {
                Writer.Write_Server("Player not found", ConsoleColor.Red);
                return;
            }
            // send chat message to all clients in the same world
            NetworkMessage reply = new NetworkMessage(MessageTypes.ToClient_Chat, message.ClientID)
                .Set(playerID).Set(text);
            HabboServer.Server.Worlds.BroadcastToWorld(reply, false);
        }

        /// <summary>
        /// Handle player Rename.
        /// </summary>
        /// <param name="message"> The network message containing the skin change data.</param>
        [NetSquareAction((ushort)MessageTypes.ToServer_Rename)]
        public static void PlayerRename(NetworkMessage message)
        {
            uint playerID = message.Serializer.GetUInt();
            string name = message.Serializer.GetString();
            // get player object
            Player player = GetPlayer(playerID);
            if (player == null)
            {
                Writer.Write_Server("Player not found", ConsoleColor.Red);
                return;
            }
            // update player name
            player.Name = name;
            // send updated player data to all clients in the same world
            NetworkMessage reply = new NetworkMessage(MessageTypes.ToClient_Rename, message.ClientID)
                .Set(playerID).Set(name);
            HabboServer.Server.Worlds.BroadcastToWorld(reply, false);
        }

        [NetSquareAction((ushort)MessageTypes.ToServer_AddGold)]
        public static void PlayerAddGold(NetworkMessage message)
        {
            uint playerID = message.Serializer.GetUInt();
            int gold = message.Serializer.GetInt();
            // get player object
            Player player = GetPlayer(playerID);
            if (player == null)
            {
                Writer.Write_Server("Player not found", ConsoleColor.Red);
                return;
            }
            // update player money
            player.Money += gold;
            // send updated player data to all clients in the same world
            NetworkMessage reply = new NetworkMessage(MessageTypes.ToClient_AddGold, message.ClientID).Set(player.ID).Set(player.Money);
            HabboServer.Server.SendToClient(reply, message.ClientID);
        }

        #region Utils
        /// <summary>
        /// Disconnect a player from the server.
        /// </summary>
        /// <param name="clientID"> The ID of the player to disconnect.</param>
        public static void PlayerDisconnect(uint clientID)
        {
            Player player = GetPlayerByClientID(clientID);
            if (player != null)
            {
                player.PlayerState = PlayerState.Disconnected;
                player.ClientID = 0;
                Database.Save();
                Writer.Write_Server($"Player {player.Name} disconnected", ConsoleColor.Green);
            }
            else
            {
                Writer.Write_Server("Disconnected player not found", ConsoleColor.Red);
            }
        }

        /// <summary>
        /// Get a player by their ID.
        /// </summary>
        /// <param name="id"> The ID of the player.</param>
        /// <returns> The player object if found, otherwise null.</returns>
        public static Player GetPlayer(uint id)
        {
            if (Database.Players.ContainsKey(id))
            {
                return Database.Players[id];
            }
            return null;
        }

        /// <summary>
        /// Get a player by their client ID.
        /// </summary>
        /// <param name="clientID"> The client ID of the player.</param>
        /// <returns> The player object if found, otherwise null.</returns>
        public static Player GetPlayerByClientID(uint clientID)
        {
            return Database.Players.Values.FirstOrDefault(p => p.ClientID == clientID);
        }
        #endregion
    }
}