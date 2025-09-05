using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Tilemaps;
using Paltry;

public class Test : MonoBehaviour {
    public Tilemap solidMap;
    public int maxSpeed;
    public int JumpForce;
    const string ServerUrl = "http://localhost:5000";
    string taskInfo = "���Ǵ����������������,�κ����Ĳ���ҪһЩĢ����ҩ��,�����ұߵĶ�Ѩ����һЩ,���ܴ�һЩ��������?��Ϊ�ر�,�һ����һЩСƿ����ҩˮ�ͽ��";
    BoxCollider2D cld;
    SpriteRenderer sr;
    void Start() {
        XLuaVM.Instance.SetInDialog("TaskState", "NotAccept");
        XLuaVM.Instance.SetInDialog("HasTask", true);
        XLuaVM.Instance.SetInDialog("Ctx.TaskInfo", taskInfo);
        XLuaVM.Instance.SetInDialog("Ctx.TaskRestDay", 32);
        EventCenter.AddListener("PlayerAskForTask", () => print("PlayerAskForTask is Triggered"));
        EventCenter.AddListener("PlayerAcceptTask", () => print("PlayerAcceptTask is Triggered"));
        EventCenter.AddListener("PlayerFinishTask", () => print("PlayerFinishTask is Triggered"));
        EventCenter.AddListener("PlayerAbandonTask", () => print("PlayerAbandonTask is Triggered"));
    }
    
    void Update() {

    }
}
