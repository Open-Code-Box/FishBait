using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FishBait;
using FishNet.Transporting;
using FishNet.Managing;

public class FishBaitFunctionTest : MonoBehaviour
{
    public Text functionDisplay;
    private NetworkManager _NetworkManager;
    private FishBaitTransport _FishBait;
    private bool _serverListUpdated = false;

    void Start()
    {
        _NetworkManager = FindObjectOfType<NetworkManager>();
        _FishBait = (FishBaitTransport)_NetworkManager.TransportManager.Transport;
        _FishBait.serverListUpdated.AddListener(ServerListUpdated);
        StartCoroutine(TestFishBait());
    }

    void ServerListUpdated() => _serverListUpdated = true;

    IEnumerator TestFishBait()
    {
        DisplayText("Waiting for FishBait to authenticate...");
        yield return new WaitUntil(() => _FishBait.IsAuthenticated());
        DisplayText("<color=lime>Authenticated!</color>");

        DisplayText("Attempting hosting a room...");
        _FishBait.serverName = "Default Server Name";
        _FishBait.extraServerData = "Default Server Data";
        _FishBait.maxServerPlayers = 5;
        _FishBait.isPublicServer = true;
        //_FishBait.StartConnection(true);
        //_FishBait.StartConnection(false);
        yield return new WaitUntil(() => _FishBait.serverId.Length > 4);
        DisplayText($"<color=lime>Room created! ID: {_FishBait.serverId}</color>");
        DisplayText("Requesting Server Data Change...");
        _FishBait.UpdateRoomName("Updated Server Name");
        _FishBait.UpdateRoomData("Updated Server Data");
        _FishBait.UpdateRoomPlayerCount(10);
        yield return new WaitForSeconds(1); // Give FishBait time to process
        DisplayText("Requesting Server List...");
        _FishBait.RequestServerList();
        yield return new WaitUntil(() => _serverListUpdated);
        foreach (var server in _FishBait.relayServerList)
            DisplayText($"Got Server: {server.serverName}, {server.serverData}, {server.maxPlayers}");
    }

    void DisplayText(string msg)
    {
        functionDisplay.text += $"\n{msg}";
    }
}
