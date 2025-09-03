using UnityEngine;
using Unity.Netcode;
using Paltry;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine.Tilemaps;

public class GameRoot : MonoSingleton<GameRoot> {
    public Tilemap SolidTilemap, PlatformTilemap;
    public static string GetLocalIP() {
        try {
            IPHostEntry IpEntry = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress item in IpEntry.AddressList) {
                //AddressFamily.InterNetwork  ipv4
                //AddressFamily.InterNetworkV6 ipv6
                if (item.AddressFamily == AddressFamily.InterNetwork) {
                    return item.ToString();
                }
            }
            return "";
        }
        catch { return ""; }
    }
    void Start() {
        UIMgr.Instance.OpenPanel(UIName.MainMenuPanel);
    }

    void Update() {

    }
}
