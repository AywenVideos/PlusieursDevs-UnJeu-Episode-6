using NetSquare.Client;
using NetSquare.Core;
using System;
using UnityEngine;

/// <summary>
/// NetSquare Client
/// </summary>
public static class NSClient
{
    #region Variables
    /// <summary>
    /// Is the client connected to the server
    /// </summary>
    public static bool IsConnected { get; private set; }
    /// <summary>
    /// ID of the client
    /// </summary>
    public static uint ClientID { get; private set; }
    /// <summary>
    /// NetSquare Client
    /// </summary>
    public static NetSquareClient Client { get; private set; }
    /// <summary>
    /// The current local time of the client
    /// </summary>
    public static float ClientTime { get; private set; }
    /// <summary>
    /// Event raised when client is connected to server
    /// </summary>
    public static event Action<uint> OnConnected;
    /// <summary>
    /// Event raised when client is disconnected from server
    /// </summary>
    public static event Action OnDisconnected;
    /// <summary>
    /// Event raised when client fail to connect to server
    /// </summary>
    public static event Action OnConnectionFail;
    /// <summary>
    /// Event raised before client connect to server
    /// </summary>
    public static event Action BeforeConnectClient;
    /// <summary>
    /// Event raised after client connect to server
    /// </summary>
    public static event Action AfterConnectClient;
    /// <summary>
    /// The current server time
    /// </summary>
    public static float ServerTime
    {
        get
        {
            return ClientTime + serverTimeOffset;
        }
        set
        {
            serverTimeOffset = value - ClientTime;
        }
    }
    /// <summary>
    /// Time offset between client and server
    /// </summary>
    private static float serverTimeOffset = 0f;
    /// <summary>
    /// Is the client in debug mode
    /// </summary>
    private static bool debug;
    /// <summary>
    /// Time offset between client and server
    /// </summary>
    private static double timeOffset = 0f;
    #endregion

    /// <summary>
    /// Static constructor
    /// </summary>
    static NSClient()
    {
        // Set the time offset between client and server
        timeOffset = new TimeSpan(DateTime.UtcNow.Ticks).TotalSeconds - Time.time;
        // Create a new NetSquare Client
        Client = new NetSquareClient();
        // Set the main thread callback for the dispatcher and register the exception event
        Client.Dispatcher.SetMainThreadCallback(NetSquareController.Instance.ExecuteInMainThread);
        Client.OnException += NetSquareController.Instance.Client_OnException;
    }

    /// <summary>
    /// Connect NetSquare Client on given port and IPAdress
    /// </summary>
    /// <param name="hostNameOrIPAdress">Hostname or IPAdress to start client on</param>
    /// <param name="Port">Port to start client on</param>
    public static void Connect(string hostNameOrIPAdress, int port, bool debugMode, NetSquareProtocoleType protocoleType, bool synchronizeUsingUDP)
    {
        debug = debugMode;
        if (debug)
        {
            Debug.Log("[NetSquare] Connecting to server on " + hostNameOrIPAdress + ":" + port + "  |  " + Client.Dispatcher.Count + " Action" + (Client.Dispatcher.Count > 1 ? "s" : "") + " registered");
        }
        BeforeConnectClient?.Invoke();
        Client.Connect(hostNameOrIPAdress, port, protocoleType, synchronizeUsingUDP);
        AfterConnectClient?.Invoke();
        Client.OnConnected += Client_Connected;
        Client.OnDisconected += Client_Disconected;
        Client.OnConnectionFail += Client_OnConnectionFail;
    }

    /// <summary>
    /// Update the client time
    /// Must be called in the Update method of the game only once
    /// By default, it is called in the Update method of the NetSquareController
    /// </summary>
    public static void UpdateTime()
    {
        ClientTime = (float)(new TimeSpan(DateTime.UtcNow.Ticks).TotalSeconds - timeOffset);
    }

    #region Event Handling
    /// <summary>
    /// Event raised when client fail to connect to server
    /// </summary>
    private static void Client_OnConnectionFail()
    {
        Client.Dispatcher.ExecuteinMainThread((action) =>
        {
            OnConnectionFail?.Invoke();
        }, null);
    }

    /// <summary>
    /// Event raised when client is disconnected from server
    /// </summary>
    private static void Client_Disconected()
    {
        if (debug)
        {
            Debug.Log("[NetSquare] Disconnected from server");
        }
        IsConnected = false;

        Client.Dispatcher.ExecuteinMainThread((action) =>
        {
            OnDisconnected?.Invoke();
        }, null);
    }

    /// <summary>
    /// Event raised when client is connected to server
    /// </summary>
    /// <param name="clientID">ID of the client</param>
    private static void Client_Connected(uint clientID)
    {
        if (debug)
        {
            Debug.Log("[NetSquare] Connected to server with ID : " + clientID);
        }
        ClientID = clientID;

        Client.Dispatcher.ExecuteinMainThread((action) =>
        {
            OnConnected?.Invoke(clientID);
            IsConnected = true;
        }, null);
    }
    #endregion

    /// <summary>
    /// Disconnect NetSquare client
    /// </summary>
    public static void Disconnect()
    {
        if (!IsConnected)
        {
            Debug.LogWarning("Trying to disconnect client that is not connected");
            return;
        }
        Client.Disconnect();
    }

    /// <summary>
    /// Send an empty message to the server with only the Head ID
    /// </summary>
    /// <param name="headID">Head ID of the message</param>
    public static void SendMessage(ushort headID)
    {
        if (!IsConnected)
        {
            Debug.LogWarning("Trying to send message to server but client is not connected");
            return;
        }
        Client.SendMessage(headID);
    }

    /// <summary>
    /// Send an empty message to the server with only the Head ID
    /// </summary>
    /// <param name="headID">Head ID of the message</param>
    public static void SendMessage(Enum headID)
    {
        if (!IsConnected)
        {
            Debug.LogWarning("Trying to send message to server but client is not connected");
            return;
        }
        Client.SendMessage(headID);
    }

    /// <summary>
    /// Send a network message to the server
    /// </summary>
    /// <param name="message">Network Message to send</param>
    public static void SendMessage(NetworkMessage message)
    {
        if (!IsConnected)
        {
            Debug.LogWarning("Trying to send message to server but client is not connected");
            return;
        }
        Client.SendMessage(message);
    }

    /// <summary>
    /// Send a network message to the server, expecting that server will reply to this message.
    /// On server reply, the callback will be invoked.
    /// </summary>
    /// <param name="message">message to send</param>
    /// <param name="callback">callback to invoke on server reply</param>
    public static void SendMessage(NetworkMessage message, NetSquareAction callback)
    {
        if (!IsConnected)
        {
            Debug.LogWarning("Trying to send message to server but client is not connected");
            return;
        }
        Client.SendMessage(message, callback);
    }

    /// <summary>
    /// Send a network message to the server, expecting that server will reply to this message.
    /// On server reply, the callback will be invoked.
    /// </summary>
    /// <param name="headID">Head ID of the message</param>
    /// <param name="callback">callback to invoke on server reply</param>
    public static void SendMessage(Enum headID, NetSquareAction callback)
    {
        if (!IsConnected)
        {
            Debug.LogWarning("Trying to send message to server but client is not connected");
            return;
        }
        NetworkMessage message = new NetworkMessage(headID, ClientID);
        Client.SendMessage(message, callback);
    }

    /// <summary>
    /// Add an action to call when server send message with the given HeadID
    /// </summary>
    /// <param name="headID">ID oh the action (given from the NetworkMessage headID)</param>
    /// <param name="callback">Callback to raise when NetworkMessage is received from  server</param>
    public static void AddAction(ushort headID, NetSquareAction callback)
    {
        Client.Dispatcher.AddHeadAction(headID, callback.Method.Name, callback);
    }

    /// <summary>
    /// Add an action to call when server send message with the given HeadID
    /// </summary>
    /// <param name="headID">ID of the action (given from the NetworkMessage headID)</param>
    /// <param name="callback">Callback to raise when NetworkMessage is received from  server</param>
    public static void AddAction(Enum headID, NetSquareAction callback)
    {
        Client.Dispatcher.AddHeadAction(headID, callback.Method.Name, callback);
    }

    /// <summary>
    /// Add an action to call when server send message with the given HeadID
    /// </summary>
    /// <param name="headID">ID of the action (given from the NetworkMessage headID)</param>
    /// <param name="actionName">[For debug] custom name of the action. If your callback methods are well named, you should not need that</param>
    /// <param name="action">Callback to raise when NetworkMessage is received from  server</param>
    public static void AddAction(ushort headID, string actionName, NetSquareAction action)
    {
        Client.Dispatcher.AddHeadAction(headID, actionName, action);
    }

    /// <summary>
    /// Add an action to call when server send message with the given HeadID
    /// </summary>
    /// <param name="headID">ID of the action (given from the NetworkMessage headID)</param>
    /// <param name="actionName">[For debug] custom name of the action. If your callback methods are well named, you should not need that</param>
    /// <param name="action">Callback to raise when NetworkMessage is received from  server</param>
    public static void AddAction(Enum headID, string actionName, NetSquareAction action)
    {
        Client.Dispatcher.AddHeadAction(headID, actionName, action);
    }
}