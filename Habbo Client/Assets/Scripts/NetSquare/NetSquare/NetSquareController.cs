using NetSquare.Core;
using NetSquare.Core.Compression;
using NetSquare.Core.Encryption;
using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace NetSquare.Client
{
    public class NetSquareController : MonoBehaviour
    {
        #region Variables
        /// <summary>
        /// Maximum messages to handle per frame.
        /// </summary>
        [Range(1, 4096)]
        [SerializeField]
        private short nbMaxMessagesByFrame = 32;
        public static NetSquareController Instance;
        private ConcurrentQueue<NetSquareActionData> netSquareActions = new ConcurrentQueue<NetSquareActionData>();
        private NetSquareActionData currentAction;
        public string IPAdress = "127.0.0.1";
        public int Port = 5555;
        public NetSquareProtocoleType ProtocoleType;
        public bool SynchronizeUsingUDP = false;
        public NetSquareCompression MessagesCompression;
        public NetSquareEncryption MessagesEncryption;
        public bool DebugMode = true;
        #endregion

        #region Unity Events
        /// <summary>
        /// Singleton to reach the client anywhere.
        /// Only one client must exists, and should never be remplaced. 
        /// We keep the first instance and self destroy this gameObject if an instance already exists
        /// </summary>
        private void Awake()
        {
            // Singleton
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else
                Destroy(gameObject);

            // load ip and port from file
            string path = Application.streamingAssetsPath + "/connection.txt";
            if (System.IO.File.Exists(path))
            {
                string[] lines = System.IO.File.ReadAllLines(path);
                if (lines.Length >= 2)
                {
                    IPAdress = lines[0];
                    Port = int.Parse(lines[1]);
                }
            }
        }

        /// <summary>
        /// Connect to server when the game start
        /// </summary>
        void Start()
        {
            if (DebugMode)
            {
                Debug.Log("Connecting");
                NSClient.OnConnected += NSClient_OnConnected;
                NSClient.OnConnectionFail += NSClient_OnConnectionFail;
            }

            ProtocoleManager.SetCompressor(MessagesCompression);
            ProtocoleManager.SetEncryptor(MessagesEncryption);
            NSClient.Connect(IPAdress, Port, DebugMode, ProtocoleType, SynchronizeUsingUDP);
            if (DebugMode)
            {
                Debug.Log(NSClient.Client.Dispatcher.GetRegisteredActionsString());
            }
        }

        /// <summary>
        /// Check at any frame if a new message have been received.
        /// If there is some, execute their related action from main thread
        /// </summary>
        private void Update()
        {
            // Execute messages actions from main thread
            short i = 0;
            while (netSquareActions.Count > 0 && i <= nbMaxMessagesByFrame)
            {
                i++;
                if (!netSquareActions.TryDequeue(out currentAction))
                    continue;

                currentAction.Action?.Invoke(currentAction.Message);
            }
        }
        #endregion

        /// <summary>
        /// Enqueue Action and message and pack it as a delegate for NetSquare Dispatcher.
        /// that way, Dispatcher will invoke network messages actions from main thread
        /// </summary>
        /// <param name="action"></param>
        /// <param name="message"></param>
        public void ExecuteInMainThread(NetSquareAction action, NetworkMessage message)
        {
            netSquareActions.Enqueue(new NetSquareActionData(action, message));
        }

        /// <summary>
        /// Whenever the application is closed, disconnect the client
        /// </summary>
        public void OnApplicationQuit()
        {
            NSClient.Client?.Disconnect();
        }

        #region Events Handlers
        /// <summary>
        /// Event raised when client throw an exception
        /// </summary>
        /// <param name="ex">The exception raised</param>
        public void Client_OnException(Exception ex)
        {
            Debug.LogError("NetSquare reception exception : \n" + ex.ToString());
        }

        /// <summary>
        /// Event raised when client fail to connect to server
        /// </summary>
        private void NSClient_OnConnectionFail()
        {
            Debug.Log("Connection failed");
        }

        /// <summary>
        /// Event raised when client is connected to server
        /// </summary>
        /// <param name="clientID">The client ID that just been connected</param>
        private void NSClient_OnConnected(uint clientID)
        {
            Debug.Log("Connected");
        }
        #endregion
    }
}