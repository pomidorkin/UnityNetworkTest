using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using System.Text;
using UnityEngine.UI;
using TMPro;
using System;

public class PasswordNetworkManager : MonoBehaviour
{

    /*
     * Passwor Protected Lobby example
     * "Connection Approval" on the NetworkManager component shold be checked
     */

    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private GameObject passwordEntryUI;
    [SerializeField] private GameObject leaveButton;

    private void Start()
    {
        NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnect;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
    }

    private void OnDestroy()
    {
        // It is important to unsubscribe from the publisher when the object is destroyed
        if (NetworkManager.Singleton == null) { return; }
        NetworkManager.Singleton.OnServerStarted -= HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnect;
        NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
    }

    public void Host()
    {
        // Subcribing to the callback to implement our logic for the connection approval check
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.StartHost(new Vector3(-2f, 0f, 0f), Quaternion.Euler(0f, 135f, 0f));
    }


    public void Client()
    {
        // This is where we set connection date to send the password as a client
        // Because we cannot send data like stings through the network, we neet to
        // convert the string (password) to the byte array
        NetworkManager.Singleton.NetworkConfig.ConnectionData =
            Encoding.ASCII.GetBytes(passwordInputField.text);


        NetworkManager.Singleton.StartClient();
    }

    public void Leave()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.StopHost();
            // Unsubcribe from the approval check
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
        } else if (NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.StopClient();
        }

        passwordEntryUI.SetActive(true);
        leaveButton.SetActive(false);
    }

    // OnClientConnected doesn't get called for the host when the host connects, so it need to be done manually
    private void HandleServerStarted()
    {
        // If we are running as a host
        if (NetworkManager.Singleton.IsHost)
        {
            HandleClientConnect(NetworkManager.Singleton.LocalClientId);
        }
    }

    // Called on the server each time when a client is connected.
    // It is calse called for the client side when the themselves connect
    private void HandleClientConnect(ulong clientId)
    {
        // If it's us, whe make the UI unactive. When we connect, turn this on and off
        if(clientId == NetworkManager.Singleton.LocalClientId)
        {
            passwordEntryUI.SetActive(false);
            leaveButton.SetActive(true);
        }
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        // When we disconnect, turn this on and off
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            passwordEntryUI.SetActive(true);
            leaveButton.SetActive(false);
        }
    }


    // In this method we write our custom logic.
    // connectionData will contain password or whatever we decide to send.
    // Note: This code doesn't get called for the host.
    private void ApprovalCheck(byte[] connectionData, ulong clientId, NetworkManager.ConnectionApprovedDelegate callback)
    {
        // connectionData is a byteArray and to check the password we need to convert it into a string
        string password = Encoding.ASCII.GetString(connectionData);

        bool approvedConnection = password == passwordInputField.text;

        // This ugly spawning implementation is just for the demonstration
        Vector3 spawnPos = Vector3.zero;
        Quaternion spawnRot = Quaternion.identity;

        // Counting connected clients and assigning them spawn coords
        switch (NetworkManager.Singleton.ConnectedClients.Count)
        {
            case 1:
                spawnPos = new Vector3(0f, 0f, 0f);
                spawnRot = Quaternion.Euler(0f, 180f, 0f);
                break;

            case 2:
                spawnPos = new Vector3(2f, 0f, 0f);
                spawnRot = Quaternion.Euler(0f, 225f, 0f);
                break;
        }

        callback(true, null, approvedConnection, null, null);
    }

}
