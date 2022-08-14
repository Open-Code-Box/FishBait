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
using System.Net.Sockets;

namespace FishBait
{
    public partial class FishBaitTransport : Transport
    {
        public bool IsAuthenticated() => _isAuthenticated;

        private void Awake()
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
                useNATPunch = false;
            else
                _directConnectModule = GetComponent<FishBaitDirectConnectModule>();

            if (transport is FishBaitTransport)
                throw new Exception("Haha real funny... Use a different transport.");

            if (_directConnectModule != null)
            {
                if (useNATPunch && !_directConnectModule.SupportsNATPunch())
                {
                    Debug.LogWarning("FishBait | NATPunch is turned on but the transport used does not support it. It will be disabled.");
                    useNATPunch = false;
                }
            }

            SetupCallbacks();

            if (connectOnAwake)
                ConnectToRelay();

            InvokeRepeating(nameof(SendHeartbeat), heartBeatInterval, heartBeatInterval);
        }

        private void SetupCallbacks()
        {
            if (_callbacksInitialized)
                return;

            _callbacksInitialized = true;

            transport.OnClientConnectionState += ClientConnectionState;
            transport.OnClientReceivedData += ClientDataRecived;
        }

        void ClientConnectionState(ClientConnectionStateArgs args)
        {
            switch (args.ConnectionState)
            {
                case LocalConnectionState.Started:
                    OnConnectedToRelay();
                    break;
                case LocalConnectionState.Stopped:
                    Disconnected();
                    break;
            }
        }

        void ClientDataRecived(ClientReceivedDataArgs args)
        {
            DataReceived(args.Data, (int)args.Channel);
        }

        private void Disconnected()
        {
            _connectedToRelay = false;
            _isAuthenticated = false;
            disconnectedFromRelay?.Invoke();
            serverStatus = "Disconnected from relay.";
        }

        private void OnConnectedToRelay()
        {
            _connectedToRelay = true;
            connectedToRelay?.Invoke();
        }

        public void ConnectToRelay()
        {
            if (!useLoadBalancer)
            {
                if (!_connectedToRelay)
                {
                    Connect(serverIP, serverPort);
                }
                else
                {
                    Debug.LogWarning("FishBait | Already connected to relay!");
                }
            }
            else
            {
                if (!_connectedToRelay)
                {
                    StartCoroutine(RelayConnect());
                }
                else
                {
                    Debug.LogWarning("FishBait | Already connected to relay!");
                }
            }
        }

        /// <summary>
        /// Connects to the desired relay
        /// </summary>
        /// <param name="serverIP"></param>
        private void Connect(string serverIP, ushort port = 7777)
        {
            // need to implement custom port
            if (transport is FishBaitTransport)
                throw new Exception("FishBait | Client to Server Transport cannot be FishBait.");

            SetTransportPort(port);

            this.serverIP = serverIP;
            serverStatus = "Connecting to relay...";
            _clientSendBuffer = new byte[transport.GetMTU(0)];

            transport.SetClientAddress(serverIP);
            transport.StartConnection(false);
        }

        public void DisconnectFromRelay()
        {
            if (IsAuthenticated())
            {
                transport.StopConnection(false);
            }
        }

        private void SendHeartbeat()
        {
            if (_connectedToRelay)
            {
                // Send a blank message with just the opcode 200, which is heartbeat
                int pos = 0;
                _clientSendBuffer.WriteByte(ref pos, 200);


                transport.SendToServer(0, new ArraySegment<byte>(_clientSendBuffer, 0, pos));


                // If NAT Puncher is initialized, send heartbeat on that as well.

                try
                {
                    if (_NATPuncher != null)
                        _NATPuncher.Send(new byte[] { 0 }, 1, _relayPuncherIP);
                }
                catch (Exception e)
                {
                    print(e);
                }

                // Check if any server-side proxies havent been used in 10 seconds, and timeout if so.
                var keys = new List<IPEndPoint>(_serverProxies.GetAllKeys());

                for (int i = 0; i < keys.Count; i++)
                {
                    if (DateTime.Now.Subtract(_serverProxies.GetByFirst(keys[i]).lastInteractionTime).TotalSeconds > 10)
                    {
                        _serverProxies.GetByFirst(keys[i]).Dispose();
                        _serverProxies.Remove(keys[i]);
                    }
                }
            }
        }

        private void DataReceived(ArraySegment<byte> segmentData, int channel)
        {
            try
            {
                var data = segmentData.Array;
                int pos = segmentData.Offset;
                // Read the opcode of the incoming data, this allows us to know what its used for.
                OpCodes opcode = (OpCodes)data.ReadByte(ref pos);

                switch (opcode)
                {
                    case OpCodes.Authenticated:
                        // Server authenticated us! That means we are fully ready to host and join servers.
                        serverStatus = "Authenticated! Good to go!";
                        _isAuthenticated = true;
                        RequestServerList();
                        break;

                    case OpCodes.AuthenticationRequest:
                        // Server requested that we send an authentication request, lets send our auth key.
                        serverStatus = "Sent authentication to relay...";
                        SendAuthKey();
                        break;

                    case OpCodes.GetData:
                        // Someone sent us a packet from their mirror over the relay
                        var recvData = data.ReadBytes(ref pos);

                        // If we are the server and the client is registered, invoke the callback
                        if (_isServer)
                        {
                            if (_connectedRelayClients.TryGetByFirst(data.ReadInt(ref pos), out int clientID))
                                OnServerDataReceived?.Invoke(clientID, new ArraySegment<byte>(recvData), channel);
                        }

                        // If we are the client, invoke the callback
                        if (_isClient)
                            OnClientDataReceived?.Invoke(new ArraySegment<byte>(recvData), channel);
                        break;

                    case OpCodes.ServerLeft:
                        // Called when we were kicked, or server was closed.
                        if (_isClient)
                        {
                            _isClient = false;
                            OnClientDisconnected?.Invoke();
                        }
                        break;

                    case OpCodes.PlayerDisconnected:
                        // Called when another player left the room.
                        if (_isServer)
                        {
                            // Get their client ID and invoke the mirror callback
                            int user = data.ReadInt(ref pos);
                            if (_connectedRelayClients.TryGetByFirst(user, out int clientID))
                            {
                                OnServerDisconnected?.Invoke(clientID);
                                _connectedRelayClients.Remove(user);
                            }
                        }
                        break;

                    case OpCodes.RoomCreated:
                        // We successfully created the room, the server also gave us the serverId of the room!
                        serverId = data.ReadString(ref pos);
                        break;

                    case OpCodes.ServerJoined:
                        // Called when a player joins the room or when we joined a room.
                        int clientId = data.ReadInt(ref pos);
                        if (_isClient)
                        {
                            // We successfully joined a room, let mirror know.
                            OnClientConnected?.Invoke();
                        }
                        if (_isServer)
                        {
                            // A client joined our room, let mirror know and setup their ID in the dictionary.
                            _connectedRelayClients.Add(clientId, _currentMemberId);
                            OnServerConnected?.Invoke(_currentMemberId);
                            _currentMemberId++;
                        }
                        break;

                    case OpCodes.DirectConnectIP:
                        // Either a client is trying to join us via NAT Punch, or we are trying to join a host over NAT punch/Direct connect.
                        var ip = data.ReadString(ref pos);
                        int port = data.ReadInt(ref pos);
                        bool attemptNatPunch = data.ReadBool(ref pos);

                        _directConnectEndpoint = new IPEndPoint(IPAddress.Parse(ip), port);

                        // Both client and server will send data to each other to open the hole.
                        if (useNATPunch && attemptNatPunch)
                        {
                            StartCoroutine(NATPunch(_directConnectEndpoint));
                        }

                        if (!_isServer)
                        {
                            // We arent the server, so lets tell the direct connect module to attempt a connection and initializing our middle man socket.
                            if (_clientProxy == null && useNATPunch && attemptNatPunch)
                            {
                                _clientProxy = new SocketProxy(_NATIP.Port - 1);
                                _clientProxy.dataReceived += ClientProcessProxyData;
                            }

                            if (useNATPunch && attemptNatPunch)
                            {
                                if (ip == LOCALHOST)
                                    _directConnectModule.JoinServer(LOCALHOST, port + 1);
                                else
                                    _directConnectModule.JoinServer(LOCALHOST, _NATIP.Port - 1);
                            }
                            else
                                _directConnectModule.JoinServer(ip, port);
                        }

                        break;

                    case OpCodes.RequestNATConnection:
                        // Called when the FishBait node would like us to establish a NAT puncher connection. Its safe to ignore if NAT punch is disabled.
                        if (useNATPunch && GetLocalIp() != null && _directConnectModule != null)
                        {
                            byte[] initalData = new byte[150];
                            int sendPos = 0;

                            initalData.WriteBool(ref sendPos, true);
                            initalData.WriteString(ref sendPos, data.ReadString(ref pos));
                            NATPunchtroughPort = data.ReadInt(ref pos);

                            if (_NATPuncher == null)
                            {
                                _NATPuncher = new UdpClient { ExclusiveAddressUse = false };
                                while (true)
                                {
                                    try
                                    {
                                        _NATIP = new IPEndPoint(IPAddress.Parse(GetLocalIp()), UnityEngine.Random.Range(16000, 17000));
                                        _NATPuncher.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                                        _NATPuncher.Client.Bind(_NATIP);
                                        break;
                                    }
                                    catch { } // Binding port is in use, keep trying :P
                                }
                            }

                            if (!IPAddress.TryParse(serverIP, out IPAddress serverAddr))
                                serverAddr = Dns.GetHostEntry(serverIP).AddressList[0];

                            _relayPuncherIP = new IPEndPoint(serverAddr, NATPunchtroughPort);

                            for (int attempts = 0; attempts < NAT_PUNCH_ATTEMPTS; attempts++)
                                _NATPuncher.Send(initalData, sendPos, _relayPuncherIP);

                            _NATPuncher.BeginReceive(new AsyncCallback(RecvData), _NATPuncher);
                        }
                        break;
                }
            }
            catch (Exception e) { print(e); }
        }

        public void UpdateRoomName(string newServerName = "My Awesome Server!")
        {
            if (_isServer)
            {
                int pos = 0;
                _clientSendBuffer.WriteByte(ref pos, (byte)OpCodes.UpdateRoomData);

                _clientSendBuffer.WriteBool(ref pos, true);
                _clientSendBuffer.WriteString(ref pos, newServerName);
                _clientSendBuffer.WriteBool(ref pos, false);
                _clientSendBuffer.WriteBool(ref pos, false);
                _clientSendBuffer.WriteBool(ref pos, false);

                transport.SendToServer(0, new ArraySegment<byte>(_clientSendBuffer, 0, pos));
            }
        }

        public void UpdateRoomData(string newServerData = "Extra Data!")
        {
            if (_isServer)
            {
                int pos = 0;
                _clientSendBuffer.WriteByte(ref pos, (byte)OpCodes.UpdateRoomData);

                _clientSendBuffer.WriteBool(ref pos, false);
                _clientSendBuffer.WriteBool(ref pos, true);
                _clientSendBuffer.WriteString(ref pos, newServerData);
                _clientSendBuffer.WriteBool(ref pos, false);
                _clientSendBuffer.WriteBool(ref pos, false);

                transport.SendToServer(0, new ArraySegment<byte>(_clientSendBuffer, 0, pos));

            }
        }

        public void UpdateRoomVisibility(bool isPublic = true)
        {
            if (_isServer)
            {
                int pos = 0;
                _clientSendBuffer.WriteByte(ref pos, (byte)OpCodes.UpdateRoomData);

                _clientSendBuffer.WriteBool(ref pos, false);
                _clientSendBuffer.WriteBool(ref pos, false);
                _clientSendBuffer.WriteBool(ref pos, true);
                _clientSendBuffer.WriteBool(ref pos, isPublic);
                _clientSendBuffer.WriteBool(ref pos, false);

                transport.SendToServer(0, new ArraySegment<byte>(_clientSendBuffer, 0, pos));

            }
        }

        public void UpdateRoomPlayerCount(int maxPlayers = 16)
        {
            if (_isServer)
            {
                int pos = 0;
                _clientSendBuffer.WriteByte(ref pos, (byte)OpCodes.UpdateRoomData);

                _clientSendBuffer.WriteBool(ref pos, false);
                _clientSendBuffer.WriteBool(ref pos, false);
                _clientSendBuffer.WriteBool(ref pos, false);
                _clientSendBuffer.WriteBool(ref pos, true);
                _clientSendBuffer.WriteInt(ref pos, maxPlayers);

                transport.SendToServer(0, new ArraySegment<byte>(_clientSendBuffer, 0, pos));

            }
        }

        private Room? GetServerForID(string serverID)
        {
            for (int i = 0; i < relayServerList.Count; i++)
            {
                if (relayServerList[i].serverId == serverID)
                    return relayServerList[i];
            }

            return null;
        }

        private void SendAuthKey()
        {
            int pos = 0;
            _clientSendBuffer.WriteByte(ref pos, (byte)OpCodes.AuthenticationResponse);
            _clientSendBuffer.WriteString(ref pos, authenticationKey);

            transport.SendToServer(0, new ArraySegment<byte>(_clientSendBuffer, 0, pos));

        }

        public enum OpCodes
        {
            Default = 0, RequestID = 1, JoinServer = 2, SendData = 3, GetID = 4, ServerJoined = 5, GetData = 6, CreateRoom = 7, ServerLeft = 8, PlayerDisconnected = 9, RoomCreated = 10,
            LeaveRoom = 11, KickPlayer = 12, AuthenticationRequest = 13, AuthenticationResponse = 14, Authenticated = 17, UpdateRoomData = 18, ServerConnectionData = 19, RequestNATConnection = 20,
            DirectConnectIP = 21
        }
    }

    [Serializable]
    public struct Room
    {
        public string serverName;
        public int maxPlayers;
        public string serverId;
        public string serverData;
        public int hostId;
        public List<int> clients;
        public int currentPlayers;
        public RelayAddress relayInfo;
    }

    [Serializable]
    public struct RelayAddress
    {
        public ushort port;
        public ushort endpointPort;
        public string address;
        public FishBaitRegions serverRegion;
    }
}
