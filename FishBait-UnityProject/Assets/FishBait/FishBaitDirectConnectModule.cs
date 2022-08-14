using FishNet.Transporting;
using System;
using System.Collections.Generic;
using UnityEngine;
using FishBait;

[RequireComponent(typeof(FishBaitTransport))]
public class FishBaitDirectConnectModule : MonoBehaviour
{
    [HideInInspector]
    public Transport directConnectTransport;
    public bool showDebugLogs;
    private FishBaitTransport fishBaitTransport;
    private int connectionId;

    void Awake()
    {
        fishBaitTransport = GetComponent<FishBaitTransport>();

        if (directConnectTransport == null)
        {
            Debug.Log("Direct Connect Transport is null!");
            return;
        }

        if (directConnectTransport is FishBaitTransport)
        {
            Debug.Log("Direct Connect Transport Cannot be the relay, silly. :P");
            return;
        }

        directConnectTransport.OnServerConnectionState += ServerConnectionState;
        directConnectTransport.OnServerReceivedData += ServerDataRecived;

        directConnectTransport.OnClientConnectionState += ClientConnectionState;
        directConnectTransport.OnClientReceivedData += ClientDataRecived;
    }

    void RemoteConnectionState(RemoteConnectionStateArgs args)
    {
        connectionId = args.ConnectionId;
    }

    void ServerConnectionState(ServerConnectionStateArgs args)
    {

        switch (args.ConnectionState)
        {
            case LocalConnectionState.Started:
                OnServerConnected(connectionId);
                break;
            case LocalConnectionState.Stopped:
                OnServerDisconnected(connectionId);
                break;
        }
    }

    void ServerDataRecived(ServerReceivedDataArgs args)
    {
        OnServerDataReceived(args.ConnectionId, args.Data, ((int)args.Channel));
    }

    void ClientConnectionState(ClientConnectionStateArgs args)
    {

        switch (args.ConnectionState)
        {
            case LocalConnectionState.Started:
                OnClientConnected();
                break;
            case LocalConnectionState.Stopped:
                OnClientDisconnected();
                break;
        }
    }

    void ClientDataRecived(ClientReceivedDataArgs args)
    {
        OnClientDataReceived(args.Data, ((int)args.Channel));
    }

    public void StartServer(int port)
    {
        if (port > 0)
            SetTransportPort(port);

        directConnectTransport.StartConnection(true);
        if (showDebugLogs)
            Debug.Log("Direct Connect Server Created!");
    }

    public void StopServer()
    {
        directConnectTransport.StopConnection(true);
    }

    public void JoinServer(string ip, int port)
    {
        if (SupportsNATPunch())
            SetTransportPort(port);
        directConnectTransport.SetClientAddress(ip);
        directConnectTransport.StartConnection(false);
    }

    public void SetTransportPort(int port)
    {
        directConnectTransport.SetPort((ushort)port);
    }

    public int GetTransportPort()
    {
        return directConnectTransport.GetPort();
    }

    public bool SupportsNATPunch()
    {
        return directConnectTransport is kcp2k.KcpTransport;
    }

    public bool KickClient(int clientID)
    {
        if (showDebugLogs)
            Debug.Log("Kicked direct connect client.");
        directConnectTransport.StopConnection(clientID, true);
        return true;
    }

    public void ClientDisconnect()
    {
        directConnectTransport.StopConnection(false);
    }

    public void ServerSend(int clientID, ArraySegment<byte> data, int channel)
    {

        directConnectTransport.SendToClient((byte)channel, data, clientID);
    }

    public void ClientSend(ArraySegment<byte> data, int channel)
    {
        directConnectTransport.SendToServer((byte)channel, data);
    }

    #region Transport Callbacks
    void OnServerConnected(int clientID)
    {
        if (showDebugLogs)
            Debug.Log("Direct Connect Client Connected");
        fishBaitTransport.DirectAddClient(clientID);
    }

    void OnServerDataReceived(int clientID, ArraySegment<byte> data, int channel)
    {
        fishBaitTransport.DirectReceiveData(data, channel, clientID);
    }

    void OnServerDisconnected(int clientID)
    {
        fishBaitTransport.DirectRemoveClient(clientID);
    }

    void OnClientConnected()
    {
        if (showDebugLogs)
            Debug.Log("Direct Connect Client Joined");

        fishBaitTransport.DirectClientConnected();
    }

    void OnClientDisconnected()
    {
        fishBaitTransport.DirectDisconnected();
    }

    void OnClientDataReceived(ArraySegment<byte> data, int channel)
    {
        fishBaitTransport.DirectReceiveData(data, channel);
    }
    #endregion
}