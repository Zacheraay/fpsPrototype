using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MLAPI;
using MLAPI.Transports.UNET;
using System;

public class MenuScript : MonoBehaviour
{
    public GameObject menuPanel;
    public InputField inputField;

    private void Start()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
    }

    private void ApprovalCheck(byte[] connectionData, ulong clientID, NetworkManager.ConnectionApprovedDelegate callback)
    {
        bool approve = false;

        //if connection data is correct, we approve the clinet
        string password = System.Text.Encoding.ASCII.GetString(connectionData);

        if(password == "password")
        {
            approve = true;
        }

        callback(true, null, approve, Vector3.zero, Quaternion.identity);
    }

    public void Host()
    {
        NetworkManager.Singleton.StartHost();
        menuPanel.SetActive(false);
    }

    public void Server()
    {
        NetworkManager.Singleton.StartServer();
        menuPanel.SetActive(false);
    }

    public void Join()
    {
        if(inputField.text.Length == 0)
        {
            inputField.text = "127.0.0.1";
        }

        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes("password");

        NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = inputField.text;
        NetworkManager.Singleton.StartClient();
        menuPanel.SetActive(false);
    }
}
