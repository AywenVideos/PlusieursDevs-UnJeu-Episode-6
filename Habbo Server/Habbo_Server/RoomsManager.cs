using Habbo_Common;
using Habbo_Common.GameEngine;
using NetSquare.Core.Messages;
using NetSquare.Core;
using NetSquare.Server.Worlds;
using System;
using System.Collections.Generic;
using NetSquare.Server.Utils;

namespace Habbo_Server
{
    public static class RoomsManager
    {
        public static Dictionary<MapNames, ushort> WorldsIds;
        public static Blocks Blocks = new Blocks();

        /// <summary>
        /// Initialize the RoomsManager and register all worlds.
        /// </summary>
        public static void Initialize()
        {
            WorldsIds = new Dictionary<MapNames, ushort>();

            // register worlds
            foreach (MapNames mapName in Enum.GetValues(typeof(MapNames)))
            {
                if (mapName != MapNames.None)
                {
                    NetSquareWorld world = HabboServer.Server.Worlds.AddWorld(mapName.ToString());
                    WorldsIds.Add(mapName, world.ID);
                }
            }

            // add custom join world messages
            HabboServer.Server.Worlds.OnSendWorldClients += (world, clientID, message) =>
            {
                // get player from clientID
                Player player = PlayerManager.GetPlayerByClientID(clientID);
                if (player != null)
                {
                    // add player data to the message
                    player.Serialize(message.Serializer);
                }

                // Send to Local client
                HabboServer.Server.SendToClient(message, clientID);
            };

            // add custom join world messages
            HabboServer.Server.Worlds.OnClientJoinWorld += (worldID, clientID, frame, message) =>
            {
                // get player from clientID
                Player player = PlayerManager.GetPlayerByClientID(clientID);
                if (player != null)
                {
                    // add player data to the message
                    player.Serialize(message.Serializer);
                }

                // Send to Local client
                HabboServer.Server.SendToClient(message, clientID);

                // send connected clients to new client but him
                List<NetworkMessage> messages = new List<NetworkMessage>();

                var world = HabboServer.Server.Worlds.GetWorld(worldID);
                lock (world.Clients)
                {
                    foreach (var client in world.Clients)
                    {
                        if (client.Key == message.ClientID)
                            continue;
                        // create new message
                        NetworkMessage connectedClientMessage = new NetworkMessage(NetSquareMessageID.ClientJoinWorld, client.Key);
                        // set Transform frame
                        client.Value.Serialize(connectedClientMessage);

                        // get player from clientID
                        Player rPlayer = PlayerManager.GetPlayerByClientID(client.Key);
                        if (rPlayer != null)
                        {
                            // add player data to the message
                            rPlayer.Serialize(connectedClientMessage.Serializer);
                        }

                        // Send to Local client
                        HabboServer.Server.SendToClient(connectedClientMessage, clientID);
                    }
                }
            };
        }

        /// <summary>
        /// Handles the PoseBlock message from the client.
        /// </summary>
        /// <param name="message"> The message received from the client.</param>
        [NetSquareAction((ushort)MessageTypes.ToServer_PoseBlock)]
        public static void PoseBlock(NetworkMessage message)
        {
            int x = message.Serializer.GetInt();
            int y = message.Serializer.GetInt();
            int blockID = message.Serializer.GetInt();

            // check if can pose block
            if (!Blocks.Contains(new Pos(x, y)))
            {
                // add block to blocks
                Blocks.blocs.Add(new Pos(x, y), blockID);
                SendBlocks(message);
            }
        }

        /// <summary>
        /// Handles the BreakBlock message from the client.
        /// </summary>
        /// <param name="message"> The message received from the client.</param>
        [NetSquareAction((ushort)MessageTypes.ToServer_BreakBlock)]
        public static void BreakBlock(NetworkMessage message)
        {
            int x = message.Serializer.GetInt();
            int y = message.Serializer.GetInt();
            // check if can break block
            if (Blocks.Contains(new Pos(x, y)))
            {
                // remove block from blocks
                Blocks.blocs.Remove(new Pos(x, y));
                SendBlocks(message);
            }
        }

        /// <summary>
        /// Sends the blocks to all clients in the world.
        /// </summary>
        /// <param name="message"> The message received from the client.</param>
        public static void SendBlocks(NetworkMessage message)
        {
            // send blocks to client
            NetworkMessage blockMessage = new NetworkMessage(MessageTypes.ToClient_SendBlocks, message.ClientID);
            Blocks.Serialize(blockMessage.Serializer);
            // send message to all clients in the world
            HabboServer.Server.Worlds.BroadcastToWorld(blockMessage, false);
        }

        /// <summary>
        /// Handles the GetBlocks message from the client.
        /// </summary>
        /// <param name="message"> The message received from the client.</param>
        [NetSquareAction((ushort)MessageTypes.ToServer_GetBlocks)]
        public static void GetBlocks(NetworkMessage message)
        {
            // send blocks to client
            NetworkMessage blockMessage = new NetworkMessage(MessageTypes.ToClient_SendBlocks, message.ClientID);
            Blocks.Serialize(blockMessage.Serializer);
            // reply to client
            message.Reply(blockMessage);
        }
    }
}