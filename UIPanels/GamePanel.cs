using Paltry;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static TMPro.TMP_InputField;

public class GamePanel : UIPanel {
    public override UICacheLevel CacheLevel => UICacheLevel.Open;
    public override UIWindowType WindowType => UIWindowType.FullScreen;
    [HideInInspector] public static GamePanel Instance;

    TMP_InputField chatInputField;
    DialogSystem dialogSystem;
    protected override void Awake() {
        base.Awake();
        if (Instance != null && Instance != this) {
            Debug.LogError($"{nameof(GamePanel)}存在多个实例");
            return;
        }
        Instance = this;
    }
    protected override void OnDestroy() {
        base.OnDestroy();
        if (Instance == this)
            Instance = null;
    }
    void Start() {
        InitUI();
        LogJoinMsg();
    }
    void Update() {
        if (Input.GetKeyDown(KeyCode.Slash)) {
            chatInputField.text = string.Empty;
            bool active = chatInputField.gameObject.activeSelf;
            chatInputField.gameObject.SetActive(!active);
            if(!active)
                chatInputField.ActivateInputField();
        }
        if (chatInputField.gameObject.activeSelf && Input.GetKeyDown(KeyCode.Escape)) {
            chatInputField.gameObject.SetActive(false);
        }
         
    }
    void InitUI() {
        chatInputField = Child<TMP_InputField>(nameof(chatInputField));
        chatInputField.onSubmit.AddListener(str => {
            GameConsoleUI.Instance.Log($"<{PlayerInfo.Local.Name}> {str}");
            chatInputField.gameObject.SetActive(false);
        });
        chatInputField.gameObject.SetActive(false);

        dialogSystem = Child<DialogSystem>(nameof(dialogSystem));
    }
    public void EnableDialogSystem(string dialogResourceName) {
        dialogSystem.OpenDialogUI(dialogResourceName);
    }
    void LogJoinMsg() {
        if (NetworkManager.Singleton.IsServer) {
            var ipStr = GameRoot.GetLocalIP();
            GameConsoleUI.Instance.Log($"主机创建成功,IP:{ipStr}");
        }
        if (NetworkManager.Singleton.IsClient) {
            GameConsoleUI.Instance.Log($"玩家 {PlayerInfo.Local.Name} 已加入游戏");
            PlayerInfo.Local.GenEnemyServerRpc();
        }
    }
    
}
