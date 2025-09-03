using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameConsoleUI : MonoBehaviour {
    public static GameConsoleUI Instance;
    public static List<string> PendingMessages = new List<string>();
    public GameConsole Console { get; private set; }
    [SerializeField] TMP_Text consoleTxt;
    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Debug.Assert(consoleTxt != null, "Console Text not assigned");
        Console = new(consoleTxt);

        foreach(string msg in PendingMessages)
            Console.Log(msg);
    }

    public void OnDestroy() {
        if (Instance == this)
            Instance = null;
    }
    public void Log(string msg) {
        if (NetworkManager.Singleton.IsServer) {
            PlayerInfo.Local.LogMessageClientRpc(msg);
        }
        else {
            PlayerInfo.Local.SubmitLogServerRpc(msg);
        }
    }

}
