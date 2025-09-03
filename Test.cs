using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Tilemaps;

public class Test : MonoBehaviour {
    public Tilemap solidMap;
    public int maxSpeed;
    public int JumpForce;
    const string ServerUrl = "http://localhost:5000";
    string taskInfo = "���Ǵ����������������,�κ����Ĳ���ҪһЩĢ����ҩ��,�����ұߵĶ�Ѩ����һЩ,���ܴ�һЩ��������?��Ϊ�ر�,�һ����һЩСƿ����ҩˮ�ͽ��";
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
