using Unity.Netcode;
using UnityEngine.UI;
using Paltry;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Netcode.Transports.UTP;
using TMPro;

public class MainMenuPanel : UIPanel {
    public override UICacheLevel CacheLevel => UICacheLevel.None;

    public override UIWindowType WindowType => UIWindowType.FullScreen;

    Button createRoomBtn, joinRoomBtn, settingBtn, quitBtn, okBtn, cancelBtn;
    TMP_Text hintTxt;
    TMP_InputField ipInputField;
    RectTransform ipInputMask;
    void Start() {
        createRoomBtn = Child<Button>(nameof(createRoomBtn));
        joinRoomBtn = Child<Button>(nameof(joinRoomBtn));
        settingBtn = Child<Button>(nameof(settingBtn));
        quitBtn = Child<Button>(nameof(quitBtn));
        ipInputMask = Child<RectTransform>(nameof(ipInputMask));
        okBtn = Child<Button>("ipInputMask/" + nameof(okBtn));
        cancelBtn = Child<Button>("ipInputMask/" + nameof(cancelBtn));
        ipInputField = Child<TMP_InputField>("ipInputMask/" + nameof(ipInputField));
        hintTxt = Child<TMP_Text>("ipInputMask/" + nameof(hintTxt));

        createRoomBtn.onClick.AddListener(CreateRoom_OnClick);
        joinRoomBtn.onClick.AddListener(JoinRoom_OnClick);
        settingBtn.onClick.AddListener(Setting_OnClick);
        quitBtn.onClick.AddListener(Quit_OnClick);
        okBtn.onClick.AddListener(Ok_OnClick);
        cancelBtn.onClick.AddListener(Cancel_OnClick);
        ipInputMask.gameObject.SetActive(false);
    }

    private void OnEnable() {
        ipInputMask?.gameObject.SetActive(false);
    }

    void CreateRoom_OnClick() {
        var unityTransport = NetworkManager.Singleton.NetworkConfig.NetworkTransport as UnityTransport;
        unityTransport.SetConnectionData(GameRoot.GetLocalIP(), 7777);
        NetworkManager.Singleton.OnServerStarted += () => {
            UIMgr.Instance.OpenPanel(UIName.GamePanel);
        };
        NetworkManager.Singleton.StartHost();
    }
    void JoinRoom_OnClick() {
        ipInputMask.gameObject.SetActive(true);
        hintTxt.text = "请输入主机IP地址";
    }
    void Setting_OnClick() {


    }
    public static void Quit_OnClick() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    void Ok_OnClick() {
        hintTxt.text = "正在加入房间...";
        UIMgr.Instance.SetInteraction(false);

        var unityTransport = NetworkManager.Singleton.NetworkConfig.NetworkTransport as UnityTransport;
        unityTransport.SetConnectionData(ipInputField.text, 7777);
        unityTransport.MaxConnectAttempts = 2;
        unityTransport.ConnectTimeoutMS = 3000;
        NetworkManager.Singleton.OnClientConnectedCallback += Net_OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += Net_OnClientDisconnect;
        NetworkManager.Singleton.StartClient();
    }


    private void Net_OnClientConnected(ulong clientId) {
        ipInputMask.gameObject.SetActive(false);
        UIMgr.Instance.OpenPanel(UIName.GamePanel);
        UIMgr.Instance.SetInteraction(true);

    }
    private void Net_OnClientDisconnect(ulong clientId) {
        hintTxt.text = "连接失败,请重试";
        UIMgr.Instance.SetInteraction(true);
    }


    void Cancel_OnClick() {
        ipInputMask.gameObject.SetActive(false);
    }
    
}
