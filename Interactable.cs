using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class Interactable : MonoBehaviour {
    [HideInInspector] public Action<PlayerInfo> OnInteract;
    [SerializeField] OnInteractPreset Preset;
    void Start() {
        OnInteract = Preset switch {
            OnInteractPreset.TalkWithNPC => TalkWithNPC,
            _ => null,
        };
    }
    void TalkWithNPC(PlayerInfo playerInfo) {
        if (Vector2.Distance(playerInfo.transform.position, transform.position) > 10f)
            return; //NPC距离玩家太远,不触发对话
        GamePanel.Instance.EnableDialogSystem(npcNameToDialogResName());
    }
    enum OnInteractPreset {
        TalkWithNPC,
    }
    string npcNameToDialogResName() => name switch {
        "Doctor" => "Dialog_Doctor"
    };
}
