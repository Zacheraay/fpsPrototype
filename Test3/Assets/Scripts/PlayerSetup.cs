using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

[RequireComponent(typeof(Player))]
public class PlayerSetup : NetworkBehaviour
{
    [SerializeField]
    Behaviour[] componentsToDisable;
    [SerializeField]
    GameObject[] gameObjectsToDisable;
    [SerializeField]
    GameObject viewModel;
    [SerializeField]
    string remoteLayerName = "RemotePlayer";


    Camera sceneCamera;

    void Start()
    {
        if (!IsLocalPlayer)
        {
            DisableComponents();
            AssignRemoteLayer();
        }
        else
        {
            for (int i = 0; i < gameObjectsToDisable.Length; i++)
            {
                gameObjectsToDisable[i].SetActive(false);
            }

            if (viewModel != null)
            {
                viewModel.gameObject.SetActive(true);
            }

            sceneCamera = Camera.main;
            if (sceneCamera != null)
            {
                sceneCamera.gameObject.SetActive(false);
            }
        }
        RegisterPlayer();

        GetComponent<Player>().Setup();
    }
    
    void RegisterPlayer()
    {
        ulong _netID = this.OwnerClientId;
        transform.name = "Player " + this.OwnerClientId;

        Player _player = GetComponent<Player>();
        GameManager.RegisterPlayer(_netID.ToString(), _player);
    }

    void AssignRemoteLayer()
    {
        gameObject.layer = LayerMask.NameToLayer(remoteLayerName);
    }

    void DisableComponents()
    {
        for (int i = 0; i < componentsToDisable.Length; i++)
        {
            componentsToDisable[i].enabled = false;
        }
    }

     void OnDisable()
    {
        if(sceneCamera != null)
        {
            sceneCamera.gameObject.SetActive(true);
        }
        GameManager.UnRegisterPlayer(transform.name);
    }
}
