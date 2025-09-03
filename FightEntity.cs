using Unity.Netcode;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class FightEntity : NetworkBehaviour {
    public int HP;

    public void OnHit(int damage) {
        if(IsClient) OnHitServerRpc(damage);
        else OnHitClientRpc(damage);
    }

    [ServerRpc(RequireOwnership = false)]
    void OnHitServerRpc(int damage) {
        OnHitClientRpc(damage);
    }
    [ClientRpc]
    void OnHitClientRpc(int damage) {
        HP -= damage;
        if (HP <= 0) {
            Destroy(gameObject);
        }
    }
}
