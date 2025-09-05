using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class HotUpdateUI : MonoBehaviour {
    [SerializeField] Button leftBtn, rightBtn;
    [SerializeField] TMP_Text hintTxt, leftBtnTxt, rightBtnTxt;
    HotUpdater hotUpdater;
    
    void SetBtn_Right_Quit() {
        rightBtn.enabled = true;
        rightBtnTxt.text = "退出游戏";
        rightBtn.ClearAndAdd(MainMenuPanel.Quit_OnClick);
    }
    public void EnterBadNetworkUI() {
        hintTxt.text = "网络异常,请重试";
        SetBtn_Right_Quit();
        leftBtn.enabled = true;
        leftBtnTxt.text = "重试";

        leftBtn.ClearAndAdd(EnterCheckingVersionUI);
    }
    public void EnterCheckingVersionUI() {
        hintTxt.text = "正在检查游戏版本...";
        SetBtn_Right_Quit();
        leftBtn.enabled = false;
        hotUpdater.CheckVersion();
    }
    public void EnterUpdateConfirmUI(string fileSize) {
        hintTxt.text = $"更新内容大小为{fileSize},是否现在开始下载?";
        SetBtn_Right_Quit();
        leftBtn.enabled = true;
        leftBtnTxt.text = "下载更新";

        leftBtn.ClearAndAdd(EnterUpdatingUI);
    }
    public void EnterUpdatingUI() {
        hintTxt.text = $"正在下载更新内容...0%";
        SetBtn_Right_Quit();
        leftBtn.enabled = false;
        hotUpdater.DownloadUpdate();
    }
    public void FinishUpdate() {
        gameObject.SetActive(false);
        XLuaVM.Instance.RunLuaInit();
        Projectile.LoadLuaLaunchTable();
    }
    /// <param name="percentage">[0,1]</param>
    public void RefreshDownloadProgress(double percentage) {
        hintTxt.text = $"正在下载更新内容...{percentage*100:F2}%";
    }
    void Start() {
        hotUpdater = new(this);
        EnterCheckingVersionUI();
    }

}
