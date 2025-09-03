using LitJson;
using Paltry;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 在text中用{x}表示外部变量
/// 在cond中,xx或!xx表示布尔变量,xx=yy或xx!=yy表示字符串或整数变量
/// </summary>
public class DialogSystem : MonoBehaviour {
    [SerializeField] TMP_Text dialogTxt; //by drag
    [SerializeField] RectTransform optionsGroup; //by drag
    (Button btn, TMP_Text txt)[] uiOptions;
    Dictionary<string, DialogNode[]> dialogCache = new();

    DialogNode lastNode = null;
    DialogNode[] curNodes = null;
    void Start() {
        uiOptions = new (Button btn, TMP_Text txt)[optionsGroup.childCount];
        for(int i = 0; i < optionsGroup.childCount; i++) {
            var child = optionsGroup.GetChild(i);
            uiOptions[i] = (child.GetComponent<Button>(), child.GetChild(0).GetComponent<TMP_Text>());
        }
        CloseDialogUI();
    }
    public void OpenDialogUI(string dialogResName) {
        gameObject.SetActive(true);
        curNodes = LoadFromCache(dialogResName);
        SetConetext();
        ShowNode(curNodes[0].id);
    }
    public void CloseDialogUI() {
        lastNode = null;
        curNodes = null;
        gameObject.SetActive(false);
    }
    void ShowNode(string nodeId) {
        DialogNode node = FindNode(nodeId);
        if(node == null) {
            CloseDialogUI();
            return;
        }
        dialogTxt.text = FormatText(node.text);
        
        ClearOptions();
        int optionCnt = 0;
        foreach(var option in node.options) {
            TryAddOption(option, ref optionCnt);
        }
        if (optionCnt == 0)
            Debug.LogError("All options don't meet the condition");
        lastNode = node;
    }
    void TryAddOption(DialogOption option, ref int optionCnt) {
        if (!CheckOptionCond(option.cond))
            return;

        var (btn, txt) = uiOptions[optionCnt];
        btn.gameObject.SetActive(true);
        txt.text = option.text;

        btn.onClick.RemoveAllListeners();
        if(option.trigger != null) {
            btn.onClick.AddListener(() => {
                ShowNode(option.next);
                EventCenter.Trigger(option.trigger);
            });
        }
        else {
            btn.onClick.AddListener(() => {
                ShowNode(option.next);
            });
        }
        optionCnt++;
    }
    bool CheckOptionCond(string cond) {
        if (cond == null) 
            return true;
        bool needReverse = false, result;
        if (cond.Contains('=')) {
            var expr = cond.Split('=');
            string left = expr[0], right = expr[1];
            if (left.EndsWith('!')) {
                needReverse = true;
                left = left[0..^1];
            }
            result = XLuaVM.Instance.GetInDialog(left).ToString() == right;
        }
        else {
            string key = cond;
            if (cond.StartsWith('!')) {
                needReverse = true;
                key = cond[1..^0];
            }
            result = (bool)XLuaVM.Instance.GetInDialog(key);
        }
        return needReverse ? !result : result;
    }
    /// <summary>
    /// 设置当前对话的变量上下文
    /// </summary>
    void SetConetext() {

    }
    DialogNode FindNode(string nodeId) {
        if (nodeId == "#back") 
            return lastNode;
        else if (nodeId == "#end") 
            return null;
        foreach (var node in curNodes) {
            if (node.id == nodeId)
                return node;
        }
        throw new KeyNotFoundException("DialogNode not found: " + nodeId);
    }
    DialogNode[] LoadFromCache(string dialogResName) {
        if (dialogCache.TryGetValue(dialogResName, out DialogNode[] result))
            return result;
        string dialogJson = AALoader.Instance.LoadAsset<TextAsset>(dialogResName).text;
        result = JsonMapper.ToObject<DialogNode[]>(dialogJson);
        dialogCache.Add(dialogResName, result);
        return result;
    }
    string FormatText(string text) {
        return Regex.Replace(text, @"\{[\w.\[\]]+\}", m => {
            string key = m.Groups[0].Value[1..^1];
            return XLuaVM.Instance.GetInDialog(key).ToString();
        });
    }
    void ClearOptions() {
        for(int i = 0; i < uiOptions.Length; i++) {
            uiOptions[i].btn.gameObject.SetActive(false);
        }
    }
}
//XLuaVM.Instance.SetInDialog("TaskState", "NotAccept");
//XLuaVM.Instance.SetInDialog("HasTask", true);
//XLuaVM.Instance.SetInDialog("Ctx.TaskInfo", taskInfo);
//XLuaVM.Instance.SetInDialog("Ctx.TaskRestDay", 32);
//EventCenter.AddListener("PlayerAskForTask", () => print("PlayerAskForTask is Triggered"));
//EventCenter.AddListener("PlayerAcceptTask", () => print("PlayerAcceptTask is Triggered"));
//EventCenter.AddListener("PlayerFinishTask", () => print("PlayerFinishTask is Triggered"));
//EventCenter.AddListener("PlayerAbandonTask", () => print("PlayerAbandonTask is Triggered"));
