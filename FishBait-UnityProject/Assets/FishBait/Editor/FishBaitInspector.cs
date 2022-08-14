using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Net;
using System.Reflection;
using System.Linq;
using System;
using FishNet.Transporting;

namespace FishBait
{
#if UNITY_EDITOR
    [CustomEditor(typeof(FishBaitTransport))]
    public class FishBaitInspector : Editor
    {
        int serverPort = 7777;
        string serverIP;
        float invalidServerIP = 0;
        bool usingLLB = false;
        FishBaitDirectConnectModule directModule;
        string[] tabs = new string[] { "FishBait Settings", "NAT Punch", "Load Balancer", "Other" };
        int currentTab = 0;
        [HideInInspector]
        public Transport transportHolder;

        public override void OnInspectorGUI()
        {
            var fishBait = (FishBaitTransport)target;
            directModule = fishBait.GetComponent<FishBaitDirectConnectModule>();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(Resources.Load<Texture>("fishbait_Logo"), GUILayout.Height(150), GUILayout.Width(200));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            if (string.IsNullOrEmpty(fishBait.loadBalancerAddress))
            {
                // First setup screen, ask if they are using LLB or just a single FishBait node.
                EditorGUILayout.HelpBox("Thank you for using FishBait!\nTo get started, please select which setup you are using.", MessageType.None);

                if (GUILayout.Button("Load Balancer Setup"))
                {
                    usingLLB = true;
                    fishBait.loadBalancerAddress = "127.0.0.1";
                    serverPort = 7070;
                }

                if (GUILayout.Button("Single FishBait Node Setup"))
                {
                    fishBait.loadBalancerAddress = "127.0.0.1";
                    fishBait.useLoadBalancer = false;
                    usingLLB = false;
                    serverIP = "172.105.109.117";
                }
            }
            else if (usingLLB)
            {
                // They said they are using LLB, configure it!
                EditorGUILayout.HelpBox("The Load Balancer is another server that is different than the FishBait node. Please enter the IP address or domain name of your Load Balancer server, along with the port.", MessageType.None);
                EditorGUILayout.HelpBox("Acceptable Examples: 127.0.0.1, mydomain.com", MessageType.Info);
                if (Time.realtimeSinceStartup - invalidServerIP < 5)
                    EditorGUILayout.HelpBox("Invalid Server Address!", MessageType.Error);

                serverIP = EditorGUILayout.TextField("Server Address", serverIP);
                serverPort = Mathf.Clamp(EditorGUILayout.IntField("Server Port", serverPort), ushort.MinValue, ushort.MaxValue);

                if (GUILayout.Button("Continue"))
                {
                    if (IPAddress.TryParse(serverIP, out IPAddress serverAddr))
                    {
                        fishBait.loadBalancerAddress = serverAddr.ToString();
                        fishBait.loadBalancerPort = (ushort)serverPort;
                        fishBait.serverIP = "127.0.0.1";
                        fishBait.useLoadBalancer = true;
                        usingLLB = false;
                        serverIP = "";
                    }
                    else
                    {
                        try
                        {
                            if (Dns.GetHostEntry(serverIP).AddressList.Length > 0)
                            {
                                fishBait.loadBalancerAddress = serverIP;
                                fishBait.loadBalancerPort = (ushort)serverPort;
                                fishBait.serverIP = "127.0.0.1";
                                usingLLB = false;
                                serverIP = "";
                            }
                            else
                                invalidServerIP = Time.realtimeSinceStartup;
                        }
                        catch
                        {
                            invalidServerIP = Time.realtimeSinceStartup;
                        }
                    }
                }
            }
            else if (fishBait.transport == null)
            {
                // next up, the actual transport. We are going to loop over all the transport types here and let them select one.
                EditorGUILayout.HelpBox("We need to use the same transport used on the server. Please select the same transport used on your FishBait Node(s)", MessageType.None);

                transportHolder = (Transport)EditorGUILayout.ObjectField(transportHolder, typeof(Transport), true);

                if (GUILayout.Button("Continue"))
                {
                    Type transportType = transportHolder.GetType();

                    var fishBaitConnectorGO = new GameObject("FishBait - Connector");
                    fishBaitConnectorGO.transform.SetParent(fishBait.transform);
                    var fishBaitConnectorAttachTransport = fishBaitConnectorGO.AddComponent(transportType);
                    Transport fishBaitConnectorTransport = fishBaitConnectorGO.GetComponent<Transport>();
                    fishBait.transport = fishBaitConnectorTransport;
                    DestroyImmediate(transportHolder);
                    transportHolder = null;
                }
            }
            else if (string.IsNullOrEmpty(fishBait.serverIP))
            {
                // Empty server IP, this is pretty important! Lets show the UI to require it.
                EditorGUILayout.HelpBox("For a single FishBait node setup, we need the IP address or domain name of your FishBait server.", MessageType.None);
                EditorGUILayout.HelpBox("Acceptable Examples: 172.105.109.117, mydomain.com", MessageType.Info);

                if (Time.realtimeSinceStartup - invalidServerIP < 5)
                    EditorGUILayout.HelpBox("Invalid Server Address!", MessageType.Error);

                serverIP = EditorGUILayout.TextField("Server Address", serverIP);
                serverPort = Mathf.Clamp(EditorGUILayout.IntField("Server Port", serverPort), ushort.MinValue, ushort.MaxValue);

                if (GUILayout.Button("Continue"))
                {
                    if (IPAddress.TryParse(serverIP, out IPAddress serverAddr))
                    {
                        fishBait.serverIP = serverAddr.ToString();
                        fishBait.SetTransportPort((ushort)serverPort);
                    }
                    else
                    {
                        try
                        {
                            if (Dns.GetHostEntry(serverIP).AddressList.Length > 0)
                            {
                                fishBait.serverIP = serverIP;
                                fishBait.SetTransportPort((ushort)serverPort);
                            }
                            else
                                invalidServerIP = Time.realtimeSinceStartup;
                        }
                        catch
                        {
                            invalidServerIP = Time.realtimeSinceStartup;
                        }
                    }
                }
            }
            else if (fishBait.NATPunchtroughPort < 0)
            {
                // NAT Punchthrough configuration.
                EditorGUILayout.HelpBox("Do you wish to use NAT Punchthrough? This can reduce load by up to 80% on your FishBait nodes, but exposes players IP's to other players.", MessageType.None);

                if (GUILayout.Button("Use NAT Punchthrough"))
                {
                    fishBait.NATPunchtroughPort = 1;
                    fishBait.useNATPunch = true;
                    fishBait.gameObject.AddComponent<FishBaitDirectConnectModule>();
                }

                if (GUILayout.Button("Do NOT use NAT Punchthrough"))
                    fishBait.NATPunchtroughPort = 1;

            }
            else if (directModule != null && directModule.directConnectTransport == null)
            {
                // NAT Punchthrough setup.
                EditorGUILayout.HelpBox("To use direct connecting, we need a transport to communicate with the other clients. Please select a transport to use.", MessageType.None);

                transportHolder = (Transport)EditorGUILayout.ObjectField(transportHolder, typeof(Transport), true);


                if (GUILayout.Button("Continue"))
                {
                    Type transportType = transportHolder.GetType();

                    var fishBaitDirectConnectGO = new GameObject("FishBait - Direct Connect");
                    fishBaitDirectConnectGO.transform.SetParent(fishBait.transform);
                    var fishBaitDirectConnectTransport = fishBaitDirectConnectGO.AddComponent(transportType);
                    Transport transport = fishBaitDirectConnectGO.GetComponent<Transport>();
                    directModule.directConnectTransport = transport;
                    DestroyImmediate(transportHolder);
                    transportHolder = null;
                }
            }
            else
            {
                // They have completed the "setup guide" Show them the main UI

                // Remove unused transports...
                foreach (var transport in fishBait.GetComponentsInChildren<Transport>())
                {
                    if (!(transport is FishBaitTransport))
                    {
                        if (transport != fishBait.transport && (directModule == null ? true : directModule.directConnectTransport != transport))
                        {
                            if (transport.gameObject == fishBait.gameObject)
                                DestroyImmediate(transport);
                            else
                                DestroyImmediate(transport.gameObject);
                        }
                    }
                }

                currentTab = GUILayout.Toolbar(currentTab, tabs);
                EditorGUILayout.Space();

                EditorGUILayout.BeginVertical("Window");
                switch (currentTab)
                {
                    case 0:
                        using (var change = new EditorGUI.ChangeCheckScope())
                        {


                            // They are in the FishBait Settings tab.
                            if (fishBait.useLoadBalancer)
                            {
                                EditorGUILayout.HelpBox("While using a Load Balancer, you don't set the FishBait node IP or port.", MessageType.Info);
                                GUI.enabled = false;
                            }
                            fishBait.serverIP = EditorGUILayout.TextField("FishBait Node IP", fishBait.serverIP);
                            fishBait.serverPort = (ushort)Mathf.Clamp(EditorGUILayout.IntField("FishBait Node Port", fishBait.serverPort), ushort.MinValue, ushort.MaxValue);
                            fishBait.endpointServerPort = (ushort)Mathf.Clamp(EditorGUILayout.IntField("Endpoint Port", fishBait.endpointServerPort), ushort.MinValue, ushort.MaxValue);

                            if (fishBait.useLoadBalancer)
                            {
                                GUI.enabled = true;
                            }

                            fishBait.authenticationKey = EditorGUILayout.TextField("FishBait Auth Key", fishBait.authenticationKey);
                            fishBait.heartBeatInterval = EditorGUILayout.Slider("Heartbeat Time", fishBait.heartBeatInterval, 0.1f, 5f);
                            fishBait.connectOnAwake = EditorGUILayout.Toggle("Connect on Awake", fishBait.connectOnAwake);
                            fishBait.transport = (Transport)EditorGUILayout.ObjectField("FishBait Transport", fishBait.transport, typeof(Transport), true);
                            if (change.changed)
                            {
                                EditorUtility.SetDirty(fishBait);
                            }
                        }
                        serializedObject.ApplyModifiedProperties();
                        break;
                    case 1:
                        // NAT punch tab.
                        if (directModule == null)
                        {
                            EditorGUILayout.HelpBox("NAT Punchthrough disabled, missing Direct Connect.", MessageType.Info);
                            if (GUILayout.Button("Add Direct Connect"))
                                fishBait.gameObject.AddComponent<FishBaitDirectConnectModule>();
                        }
                        else
                        {
                            fishBait.useNATPunch = EditorGUILayout.Toggle("Use NAT Punch", fishBait.useNATPunch);
                            GUI.enabled = true;
                            directModule.directConnectTransport = (Transport)EditorGUILayout.ObjectField("Direct Transport", directModule.directConnectTransport, typeof(Transport), true);
                        }
                        serializedObject.ApplyModifiedProperties();
                        break;
                    case 2:
                        // Load balancer tab
                        fishBait.useLoadBalancer = EditorGUILayout.Toggle("Use Load Balancer", fishBait.useLoadBalancer);
                        if (!fishBait.useLoadBalancer)
                            GUI.enabled = false;
                        fishBait.loadBalancerAddress = EditorGUILayout.TextField("Load Balancer Address", fishBait.loadBalancerAddress);
                        fishBait.loadBalancerPort = (ushort)Mathf.Clamp(EditorGUILayout.IntField("Load Balancer Port", fishBait.loadBalancerPort), ushort.MinValue, ushort.MaxValue);
                        fishBait.region = (FishBaitRegions)EditorGUILayout.EnumPopup("Node Region", fishBait.region);
                        if (!fishBait.useLoadBalancer)
                            GUI.enabled = true;
                        serializedObject.ApplyModifiedProperties();
                        break;
                    case 3:
                        // Other tab...

                        GUI.enabled = false;
                        EditorGUILayout.TextField("Server Status", fishBait.serverStatus);
                        EditorGUILayout.TextField("Server ID", string.IsNullOrEmpty(fishBait.serverId) ? "Not Hosting." : fishBait.serverId);
                        GUI.enabled = true;

                        EditorGUILayout.Space();

                        fishBait.serverName = EditorGUILayout.TextField("Server Name", fishBait.serverName);
                        fishBait.extraServerData = EditorGUILayout.TextField("Extra Server Data", fishBait.extraServerData);
                        fishBait.maxServerPlayers = EditorGUILayout.IntField("Max Server Players", fishBait.maxServerPlayers);
                        fishBait.isPublicServer = EditorGUILayout.Toggle("Is Public Server", fishBait.isPublicServer);

                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("connectedToRelay"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("disconnectedFromRelay"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("serverListUpdated"));
                        serializedObject.ApplyModifiedProperties();
                        break;
                }
                EditorGUILayout.EndVertical();
            }
        }
    }
#endif
}

