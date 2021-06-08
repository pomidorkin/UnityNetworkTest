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

   public void Host()
    {
        // Subcribing to the callback to implement our logic for the connection approval check
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.StartHost();
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

    }


    // In this method we write our custom logic
    // connectionData will contain password or whatever we decide to send
    private void ApprovalCheck(byte[] connectionData, ulong clientId, NetworkManager.ConnectionApprovedDelegate callback)
    {
        // connectionData is a byteArray and to check the password we need to convert it into a string
        string password = Encoding.ASCII.GetString(connectionData);

        bool approvedConnection = password == passwordInputField.text;

        callback(true, null, approvedConnection, null, null);
    }

}
