using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Tilemaps;

public class Test : MonoBehaviour {
    public Tilemap solidMap;
    public int maxSpeed;
    public int JumpForce;
    const string ServerUrl = "http://localhost:5000";
    string taskInfo = "我们村子最近有人生病了,治好它的病需要一些蘑菇做药材,村子右边的洞穴里有一些,你能带一些给我们吗?作为回报,我会给你一些小瓶治疗药水和金币";
    void Start() {
        
    }
    IEnumerator Download() {
        UnityWebRequest req = UnityWebRequestAssetBundle.GetAssetBundle(ServerUrl + "/gameres/entity_prefabs");
        print("Start downloading");
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success) {
            Debug.LogError("Failed to download bundle: " + req.error);
            yield break;
        }

        AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(req);

        GameObject prefab = bundle.LoadAsset<GameObject>("Enemy");
        Instantiate(prefab);

    }
    void Update() {

    }
}
