using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Messaging;

public class Player : NetworkBehaviour
{
    private NetworkVariableBool _isDead = new NetworkVariableBool(false);
    public bool isDead
    {
        get { return _isDead.Value; }
        protected set { _isDead.Value = value; }
    }

    [SerializeField]
    private int maxHealth = 100;

    private NetworkVariableInt currentHealth = new NetworkVariableInt();

    [SerializeField]
    private GameObject viewModel;

    [SerializeField]
    private Behaviour[] disableOnDeath;
    private bool[] wasEnabled;

    public void Setup()
    {
        wasEnabled = new bool[disableOnDeath.Length];
        for(int i = 0; i < wasEnabled.Length; i++)
        {
            wasEnabled[i] = disableOnDeath[i].enabled;
        }

        SetDefaults();
        SetDefaultsClientRpc();

        SpawnClientRpc();
    }

    public void TakeDamage(int _amount)
    {
        if(isDead)
            return;

        currentHealth.Value -= _amount;

        Debug.Log(transform.name + " now has " + currentHealth.Value + " health.");

        if(currentHealth.Value <= 0)
        {
            isDead = true;
            
            DieClientRpc();

            StartCoroutine(Respawn());
        }
    }

    [ClientRpc]
    private void DieClientRpc()
    {
        for(int i = 0; i < disableOnDeath.Length; i++)
        {
            disableOnDeath[i].enabled = false;
        }

        if(viewModel != null && IsLocalPlayer)
        {
            viewModel.SetActive(false);
        }

        this.transform.Find("Body").GetComponent<MeshRenderer>().enabled = false;
        this.transform.Find("Head").GetComponent<MeshRenderer>().enabled = false;
        this.transform.Find("Head").Find("Eyes").GetComponent<MeshRenderer>().enabled = false;
        Collider _collider = GetComponent<Collider>();
        if (_collider != null)
        {
            _collider.enabled = false;
        }

        Debug.Log(transform.name + " is dead");
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(GameManager.instance.matchSettings.respawnTime);

        SetDefaults();
        SetDefaultsClientRpc();

        SpawnClientRpc();

        Debug.Log(transform.name + " respawned");
    }

    [ClientRpc]
    public void SpawnClientRpc()
    {
        int randomSpawn = Mathf.FloorToInt(Random.Range(0, GameManager.instance.spawnPoints.Length));

        int _playerID = (int)this.OwnerClientId;

        if(_playerID == 0)
        {
            _playerID = 1;
        }


        if (_playerID % 2 == 0 && randomSpawn % 2 != 0)
        {
            randomSpawn--;
        }
        if (_playerID % 2 != 0 && randomSpawn % 2 == 0)
        {
            randomSpawn++;
        }

        GetComponent<PlayerMovement>().controller.enabled = false;
        transform.position = GameManager.instance.spawnPoints[randomSpawn].position;
        GetComponent<PlayerMovement>().controller.enabled = true;
        transform.rotation = GameManager.instance.spawnPoints[randomSpawn].rotation;

        this.transform.Find("Body").GetComponent<MeshRenderer>().enabled = true;
        this.transform.Find("Head").GetComponent<MeshRenderer>().enabled = true;
        this.transform.Find("Head").Find("Eyes").GetComponent<MeshRenderer>().enabled = true;
    }

    public void SetDefaults()
    {
        isDead = false;

        currentHealth.Value = maxHealth;
    }

    [ClientRpc]
    public void SetDefaultsClientRpc()
    {
        for (int i = 0; i < disableOnDeath.Length; i++)
        {
            disableOnDeath[i].enabled = wasEnabled[i];
        }

        if (viewModel != null && IsLocalPlayer)
        {
            viewModel.SetActive(true);
        }

        Collider _collider = GetComponent<Collider>();
        if (_collider != null)
        {
            _collider.enabled = true;
        }
    }

    [ServerRpc]
    public void SyncScaleServerRpc(float _scale)
    {
        foreach(Player _player in GameManager.players.Values)
        {
            if(_player != this)
            {
                foreach(string _netID in GameManager.players.Keys)
                {
                    if(GameManager.players[_netID] == this)
                    {
                        _player.SyncScaleClientRpc(_scale, _netID);
                    }
                }
            }
            
        }
    }

    [ClientRpc]
    public void SyncScaleClientRpc(float _scale, string _netID)
    {
        GameManager.players[_netID].transform.Find("Body").transform.localScale = new Vector3(0.6f, _scale, 0.6f);
        GameManager.players[_netID].transform.Find("Body").transform.localPosition = new Vector3(0f, _scale, 0f);
        GameManager.players[_netID].transform.Find("Hitbox").transform.Find("Body_Hitbox").transform.localScale = new Vector3(0.6f, _scale, 0.6f);
        GameManager.players[_netID].transform.Find("Hitbox").transform.Find("Body_Hitbox").transform.localPosition = new Vector3(0f, _scale, 0f);
    }
}
