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
    string taskInfo = "我们村子最近有人生病了,治好它的病需要一些蘑菇做药材,村子右边的洞穴里有一些,你能带一些给我们吗?作为回报,我会给你一些小瓶治疗药水和金币";
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
