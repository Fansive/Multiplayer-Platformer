using System.Linq;
using System.IO;
using Paltry;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using ContrastTable = System.Collections.Generic.Dictionary<string, (int size, string md5)>;

internal class HotUpdater {
    const string ServerUrl = "http://localhost:5000";
    const string ContrastFileName = "contrast.txt";
    HotUpdateUI ui;
    UpdateList updateList;
    ContrastTable remoteContrast;
    string remoteContrastTxt;
    public HotUpdater(HotUpdateUI ui) {
        this.ui = ui;
    }
    struct UpdateList {
        public List<string> ToDownload, ToDelete;
        public void DeleteUnnecessary() {
            foreach(var f in ToDelete) {
                var path = Path.Combine(Application.persistentDataPath, f);
                if (File.Exists(path))
                    File.Delete(path);
            }
        }
        public int GetToDownloadSize(ContrastTable remote) 
            => ToDownload.Sum(x => remote[x].size);

        public string GetToDownloadSizeStr(ContrastTable remote) {
            int sum = GetToDownloadSize(remote);
            return sum switch {
                <= (1 << 10) - 1 => sum + "B",
                <= (1 << 20) - 1 => $"{(double)sum / (1<<10):F2}KiB",
                <= (1 << 30) - 1 => $"{(double)sum / (1<<20):F2}MiB",
                _ => $"{(double)sum / (1<<30):F2}GiB"
            };
        }
    }
    
    public void CheckVersion() {
        MonoAgent.Instance.StartCoroutine(CheckVersion_Core());   
    }
    public void DownloadUpdate() {
        MonoAgent.Instance.StartCoroutine(DownloadUpdate_Core());

    }

    ContrastTable LoadLocalLatestContrast() {
        string streamPath = Path.Combine(Application.streamingAssetsPath, ContrastFileName);
        string persistentPath = Path.Combine(Application.persistentDataPath, ContrastFileName);
        string targetPath = File.Exists(persistentPath) ? persistentPath
            : (File.Exists(streamPath) ? streamPath : null);
        return targetPath == null ? null : BuildContrastTable(File.ReadAllText(targetPath));
    }
    void FillUpdateList(ContrastTable localContrast, ContrastTable remoteContrast) {
        updateList = new UpdateList() { ToDownload = new() };
        foreach(var remote in remoteContrast) {
            if(localContrast.TryGetValue(remote.Key, out var localVal)) {
                if (!IsResEqual(localVal, remote.Value))
                    updateList.ToDownload.Add(remote.Key);
            }
            else 
                updateList.ToDownload.Add(remote.Key);
            localContrast.Remove(remote.Key);
        }
        updateList.ToDelete = new(localContrast.Keys);
    }
    IEnumerator CheckVersion_Core() {
        using UnityWebRequest req = UnityWebRequest.Get(ServerUrl + "/contrast");
        yield return req.SendWebRequest();
        if(req.result != UnityWebRequest.Result.Success) {
            ReportError(req);
            yield break;
        }
        var localContrast = LoadLocalLatestContrast();
        remoteContrastTxt = req.downloadHandler.text;
        remoteContrast = BuildContrastTable(remoteContrastTxt);
        FillUpdateList(localContrast, remoteContrast);

        updateList.DeleteUnnecessary();
        if(updateList.ToDownload.Count == 0) {
            ui.FinishUpdate();
            yield break;
        }
        string fileSize = updateList.GetToDownloadSizeStr(remoteContrast);
        ui.EnterUpdateConfirmUI(fileSize);
    }

    IEnumerator DownloadUpdate_Core() {
        double curSize = 0, totalSize = updateList.GetToDownloadSize(remoteContrast);
        foreach(var file in updateList.ToDownload) {
            int retryCnt = 0;
            var targetPath = Path.Combine(Application.persistentDataPath, file);
            while (true) {
                using UnityWebRequest req = new (ServerUrl + "/gameres/" + file, 
                    UnityWebRequest.kHttpVerbGET) {
                    downloadHandler = new DownloadHandlerFile(targetPath) {
                        removeFileOnAbort = true
                    }
                };
                req.SendWebRequest();
                while (!req.isDone) {
                    double progress = (curSize + req.downloadProgress * remoteContrast[file].size) / totalSize;
                    ui.RefreshDownloadProgress(progress);
                    yield return new WaitForSeconds(0.02f);
                }

                if (req.result == UnityWebRequest.Result.Success) {
                    curSize += remoteContrast[file].size;
                    break;
                }
                if(retryCnt++ >= 3) {
                    ReportError(req);
                    yield break;
                }
            }
        }
        File.WriteAllText(Path.Combine(Application.persistentDataPath, ContrastFileName)
            , remoteContrastTxt);
        ui.FinishUpdate();
    }

    #region Helper
    ContrastTable BuildContrastTable(string contrastFile) {
        ContrastTable table = new();
        foreach (var entry in contrastFile.Split("\r\n")) {
            var info = entry.Split(' ');
            table.Add(info[0], (int.Parse(info[1]), info[2]));
        }
        return table;
    }
    bool IsResEqual((int size, string md5)a, (int size, string md5) b) {
        if(a.size != b.size) return false;
        return a.md5 == b.md5;
    }
    void ReportError(UnityWebRequest req) {
        Debug.LogError($"Network Error:{req.error} Code:{req.responseCode}");
        ui.EnterBadNetworkUI();
    }
    #endregion
}
