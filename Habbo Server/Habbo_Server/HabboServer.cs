using Habbo_Common;
using Habbo_Server.Datas;
using Habbo_Server.Utils;
using NetSquare.Core;
using NetSquare.Server;
using NetSquare.Server.Utils;
using System;

namespace Habbo_Server
{
    public static class HabboServer
    {
        public static NetSquareServer Server;
        static bool debug = false;

        public static void Start()
        {
            // Initialize the server
            Server = new NetSquareServer(NetSquareProtocoleType.TCP);
            Server.DrawHeaderOverrideCallback = (header) =>
            {
                Writer.Title("Abbos Server ");

                Writer.Write(@"   _____ ___.  ___.                  ", ConsoleColor.White, false);
                Writer.Write(@"_________                                ", ConsoleColor.Red, false);
                Writer.Write("\n", ConsoleColor.White, false);

                Writer.Write(@"  /  _  \\_ |__\_ |__   ____  ______", ConsoleColor.White, false);
                Writer.Write(@"/   _____/ ______________  __ ___________ ", ConsoleColor.Red, false);
                Writer.Write("\n", ConsoleColor.White, false);

                Writer.Write(@" /  /_\  \| __ \| __ \ /  _ \/  ___/", ConsoleColor.White, false);
                Writer.Write(@"\_____  \_/ __ \_  __ \  \/ // __ \_  __ \", ConsoleColor.Red, false);
                Writer.Write("\n", ConsoleColor.White, false);

                Writer.Write(@"/    |    \ \_\ \ \_\ (  <_> )___ \ ", ConsoleColor.White, false);
                Writer.Write(@"/        \  ___/|  | \/\   /\  ___/|  | \/", ConsoleColor.Red, false);
                Writer.Write("\n", ConsoleColor.White, false);

                Writer.Write(@"\____|__  /___  /___  /\____/____  >", ConsoleColor.White, false);
                Writer.Write(@"_______  /\___  >__|    \_/  \___  >__|   ", ConsoleColor.Red, false);
                Writer.Write("\n", ConsoleColor.White, false);

                Writer.Write(@"        \/    \/    \/           \/ ", ConsoleColor.White, false);
                Writer.Write(@"        \/     \/                 \/        ", ConsoleColor.Red, false);
                Writer.Write("\n", ConsoleColor.White, false);

                Writer.Write("                         by ", ConsoleColor.White, false);
                Writer.Write("Keks\n", ConsoleColor.Red, false);

            };
            Writer.Title("Abbos Server - Keks");
            Server.Start(5555);
            Writer.Title("Abbos Server - Keks");

            // Register message handlers
            if (debug)
            {
                Server.OnMessageReceived += (message) =>
                {
                    Console.WriteLine($"Received from {message.ClientID} ({(MessageTypes)message.HeadID}): {message.Serializer.Length} bytes");
                };
                Server.OnMessageSend += (message) =>
                {
                    NetworkMessage sendedMsg = new NetworkMessage(message);
                    Console.WriteLine($"Send ({(MessageTypes)sendedMsg.HeadID}): {sendedMsg.Serializer.Length} bytes");
                };
                Server.OnClientConnected += (clientID) =>
                {
                    Console.WriteLine($"Client connected: {clientID}");
                };
                Server.OnClientDisconnected += (clientID) =>
                {
                    PlayerManager.PlayerDisconnect(clientID);
                };
            }

            // Initialize
            ConfigurationManager.Load(false);
            Database.Connect();
            RoomsManager.Initialize();

            Console.WriteLine("Server started on port 5555");
            Console.ReadLine();
        }
    }
}
