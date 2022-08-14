using FishNet.Managing;
using FishNet.Managing.Logging;
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
        #region Forward everything to Transport

        

        public override event Action<ClientConnectionStateArgs> OnClientConnectionState
        {
            add
            {
                transport.OnClientConnectionState += value;
            }

            remove
            {
                transport.OnClientConnectionState -= value;
            }
        }

        public override event Action<ServerConnectionStateArgs> OnServerConnectionState
        {
            add
            {
                transport.OnServerConnectionState += value;
            }

            remove
            {
                transport.OnServerConnectionState -= value;
            }
        }

        public override event Action<RemoteConnectionStateArgs> OnRemoteConnectionState
        {
            add
            {
                transport.OnRemoteConnectionState += value;
            }

            remove
            {
                transport.OnRemoteConnectionState -= value;
            }
        }

        public override event Action<ClientReceivedDataArgs> OnClientReceivedData
        {
            add
            {
                transport.OnClientReceivedData += value;
            }

            remove
            {
                transport.OnClientReceivedData -= value;
            }
        }

        public override event Action<ServerReceivedDataArgs> OnServerReceivedData
        {
            add
            {
                transport.OnServerReceivedData += value;
            }

            remove
            {
                transport.OnServerReceivedData -= value;
            }
        }

        public override string GetConnectionAddress(int connectionId)
        {
            return transport.GetConnectionAddress(connectionId);
        }

        public override LocalConnectionState GetConnectionState(bool server)
        {
            return transport.GetConnectionState(server);
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
        #endregion


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
        public void SetTransportPort(ushort port)
        {
            transport.SetPort(port);
        }
    }
}

