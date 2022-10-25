using FishNet.Managing;
using FishNet.Managing.Logging;
using FishNet.Managing.Server;
using FishNet.Serializing;
using FishNet.Transporting;
using FishNet.Utility.Extension;
using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using FishNet.Transporting.Tugboat;
using FishNet.Managing.Transporting;
using FishNet.Transporting.Multipass;

namespace FishBait
{
    [DefaultExecutionOrder(1001)]
    public partial class FishBaitTransport : Transport
    {
        public override bool Equals(object other)
        {
            return base.Equals(other);
        }

        public override string GetClientAddress()
        {
            return base.GetClientAddress();
        }

        public override string GetConnectionAddress(int connectionId)
        {
            throw new NotImplementedException();
        }

        public override LocalConnectionState GetConnectionState(bool server)
        {
            throw new NotImplementedException();
        }

        public override RemoteConnectionState GetConnectionState(int connectionId)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override int GetMaximumClients()
        {
            return base.GetMaximumClients();
        }

        public override int GetMTU(byte channel)
        {
            throw new NotImplementedException();
        }

        public override ushort GetPort()
        {
            return base.GetPort();
        }

        public override string GetServerBindAddress()
        {
            return base.GetServerBindAddress();
        }

        public override string GetServerBindAddress(IPAddressType addressType)
        {
            return base.GetServerBindAddress(addressType);
        }

        public override float GetTimeout(bool asServer)
        {
            return base.GetTimeout(asServer);
        }

        public override void HandleClientConnectionState(ClientConnectionStateArgs connectionStateArgs)
        {
            throw new NotImplementedException();
        }

        public override void HandleClientReceivedDataArgs(ClientReceivedDataArgs receivedDataArgs)
        {
            throw new NotImplementedException();
        }

        public override void HandleRemoteConnectionState(RemoteConnectionStateArgs connectionStateArgs)
        {
            throw new NotImplementedException();
        }

        public override void HandleServerConnectionState(ServerConnectionStateArgs connectionStateArgs)
        {
            throw new NotImplementedException();
        }

        public override void HandleServerReceivedDataArgs(ServerReceivedDataArgs receivedDataArgs)
        {
            throw new NotImplementedException();
        }

        public override void Initialize(NetworkManager networkManager, int transportIndex)
        {
            base.Initialize(networkManager, transportIndex);
        }

        public override bool IsLocalTransport(int connectionid)
        {
            return base.IsLocalTransport(connectionid);
        }

        public override void IterateIncoming(bool server)
        {
            throw new NotImplementedException();
        }

        public override void IterateOutgoing(bool server)
        {
            throw new NotImplementedException();
        }

        public override void SendToClient(byte channelId, ArraySegment<byte> segment, int connectionId)
        {
            throw new NotImplementedException();
        }

        public override void SendToServer(byte channelId, ArraySegment<byte> segment)
        {
            throw new NotImplementedException();
        }

        public override void SetClientAddress(string address)
        {
            base.SetClientAddress(address);
        }

        public override void SetMaximumClients(int value)
        {
            base.SetMaximumClients(value);
        }

        public override void SetPort(ushort port)
        {
            base.SetPort(port);
        }

        public override void SetServerBindAddress(string address)
        {
            base.SetServerBindAddress(address);
        }

        public override void SetServerBindAddress(string address, IPAddressType addressType)
        {
            base.SetServerBindAddress(address, addressType);
        }

        public override void SetTimeout(float value, bool asServer)
        {
            base.SetTimeout(value, asServer);
        }

        public void SetTransportPort(ushort port)
        {
            transport.SetPort(port);
        }

        public override void Shutdown()
        {
            throw new NotImplementedException();
        }

        public override bool StartConnection(bool server)
        {
            throw new NotImplementedException();
        }

        public override bool StopConnection(bool server)
        {
            throw new NotImplementedException();
        }

        public override bool StopConnection(int connectionId, bool immediately)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return base.ToString();
        }


        /// <summary>Called by Transport when a new client connected to the server.</summary>
        public Action<int> OnServerConnected = (connId) => Debug.LogWarning("OnServerConnected called with no handler");
        /// <summary>Called by Transport when a client disconnected from the server.</summary>
        public Action<int> OnServerDisconnected = (connId) => Debug.LogWarning("OnServerDisconnected called with no handler");
        /// <summary>Called by Transport when the server received a message from a client.</summary>
        public Action<int, ArraySegment<byte>, int> OnServerDataReceived = (connId, data, channel) => Debug.LogWarning("OnServerDataReceived called with no handler");
        /// <summary>Called by Transport when the client received a message from the server.</summary>
        public Action<ArraySegment<byte>, int> OnClientDataReceived = (data, channel) => Debug.LogWarning("OnClientDataReceived called with no handler");
        /// <summary>Called by Transport when the client connected to the server.</summary>
        public Action OnClientConnected = () => Debug.LogWarning("OnClientConnected called with no handler");
        /// <summary>Called by Transport when the client disconnected from the server.</summary>
        public Action OnClientDisconnected = () => Debug.LogWarning("OnClientDisconnected called with no handler");

        public override event Action<ClientConnectionStateArgs> OnClientConnectionState;
        public override event Action<ServerConnectionStateArgs> OnServerConnectionState;
        public override event Action<RemoteConnectionStateArgs> OnRemoteConnectionState;
        public override event Action<ClientReceivedDataArgs> OnClientReceivedData;
        public override event Action<ServerReceivedDataArgs> OnServerReceivedData;


    }
}

