using FishNet.Transporting;
using FishNet.Managing;
using FishNet.Managing.Logging;
using FishNet.Serializing;
using FishNet.Utility.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Test : Transport
{
    public Transport transport;

    private int fishBaitId;
    private int transportId;

    public override event Action<ClientConnectionStateArgs> OnClientConnectionState;
    public override event Action<ServerConnectionStateArgs> OnServerConnectionState;
    public override event Action<RemoteConnectionStateArgs> OnRemoteConnectionState;
    public override event Action<ClientReceivedDataArgs> OnClientReceivedData;
    public override event Action<ServerReceivedDataArgs> OnServerReceivedData;

    public override string GetConnectionAddress(int connectionId)
    {
        return transport.GetConnectionAddress(connectionId);
    }

    public override LocalConnectionState GetConnectionState(bool server)
    {
        return GetConnectionState(server);
    }

    public override RemoteConnectionState GetConnectionState(int connectionId)
    {
        return transport.GetConnectionState(connectionId);
    }

    public override int GetMTU(byte channel)
    {
        return transport.GetMTU(channel);
    }

    public override void HandleClientConnectionState(ClientConnectionStateArgs connectionStateArgs)
    {
        transport.HandleClientConnectionState(connectionStateArgs);
    }

    public override void HandleClientReceivedDataArgs(ClientReceivedDataArgs receivedDataArgs)
    {
        transport.HandleClientReceivedDataArgs(receivedDataArgs);
    }

    public override void HandleRemoteConnectionState(RemoteConnectionStateArgs connectionStateArgs)
    {
        transport.HandleRemoteConnectionState(connectionStateArgs);
    }

    public override void HandleServerConnectionState(ServerConnectionStateArgs connectionStateArgs)
    {
        transport.HandleServerConnectionState(connectionStateArgs);
    }

    public override void HandleServerReceivedDataArgs(ServerReceivedDataArgs receivedDataArgs)
    {
        transport.HandleServerReceivedDataArgs(receivedDataArgs);
    }

    public override void IterateIncoming(bool server)
    {
        transport.IterateIncoming(server);
    }

    public override void IterateOutgoing(bool server)
    {
        transport.IterateOutgoing(server);
    }

    public override void SendToClient(byte channelId, ArraySegment<byte> segment, int connectionId)
    {
        transport.SendToClient(channelId, segment, connectionId);
    }

    public override void SendToServer(byte channelId, ArraySegment<byte> segment)
    {
        transport.SendToServer(channelId, segment);
    }

    public override void Shutdown()
    {
        transport.Shutdown();
    }

    public override bool StartConnection(bool server)
    {
        return transport.StartConnection(server);
    }

    public override bool StopConnection(bool server)
    {
        return transport.StopConnection(server);
    }

    public override bool StopConnection(int connectionId, bool immediately)
    {
        return transport.StopConnection(connectionId, immediately);
    }

    public override void Initialize(NetworkManager networkManager, int transportIndex)
    {
        base.Initialize(networkManager, transportIndex);

        if (transport == null)
        {
            if (base.NetworkManager.CanLog(LoggingType.Error))
                Debug.LogError($"No transport was set for use with FishBait");
        }

        transport.Initialize(networkManager, 0);
        transport.OnClientConnectionState += FishBait_OnClientConnectionState;
    }


    public void FishBait_OnClientConnectionState(ClientConnectionStateArgs args)
    {
        OnClientConnectionState?.Invoke(args);
    }

    public void FishBait_OnServerConnectionState(ServerConnectionStateArgs args)
    {
        OnServerConnectionState?.Invoke(args);
    }

    public void FishBait_OnRemoteConnectionState(RemoteConnectionStateArgs args)
    {
        OnRemoteConnectionState?.Invoke(args);
    }

    public void FishBait_OnClientReceivedData(ClientReceivedDataArgs args)
    {
        OnClientReceivedData?.Invoke(args);
    }

    public void FishBait_OnServerReceivedData(ServerReceivedDataArgs args)
    {
        OnServerReceivedData?.Invoke(args);
    }

    private void OnDestroy()
    {
        transport.Shutdown();
    }
}
