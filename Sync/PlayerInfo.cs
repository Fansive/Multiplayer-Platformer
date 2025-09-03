using System.Collections.Generic;
using Paltry;
using UnityEngine;
using Unity.Netcode;
using System;

class PlayerInfo : NetworkBehaviour {
    public string Name = "Player";
    public static PlayerInfo Local;
    public static Dictionary<ulong, PlayerInfo> PlayerList = new();

    public PlayerControlSync PlayerControl { get; private set; }
    //Awake->OnNetworkSpawn->Start
    public override void OnNetworkSpawn() {
        if(IsOwner) {
            Local = this;
        }
        PlayerList.Add(OwnerClientId, this);
    }
    private void Start() {
        PlayerControl = GetComponent<PlayerControlSync>();
    }

    public override void OnNetworkDespawn() {
        PlayerList.Remove(OwnerClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SubmitLogServerRpc(string msg) {
        LogMessageClientRpc(msg);
    }
    [ClientRpc]
    public void LogMessageClientRpc(string msg) { //log to every client(include host)
        if(GameConsoleUI.Instance == null)
            GameConsoleUI.PendingMessages.Add(msg);
        else
            GameConsoleUI.Instance.Console.Log(msg);
    }

    [ServerRpc]
    public void GenEnemyServerRpc() {
        var enemyPrefab = AALoader.Instance.LoadAsset<GameObject>("Enemy");
        GameObject enemy = Instantiate(enemyPrefab, Vector3.zero, Quaternion.identity);
        enemy.GetComponent<NetworkObject>().Spawn(true); // true = spawn for all clients
    }
}