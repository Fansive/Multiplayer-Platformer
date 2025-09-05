using Paltry;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using XLua;

[CSharpCallLua]
public delegate void LaunchDelegate(Projectile projectile, Vector2 dir);

[LuaCallCSharp]
public class Projectile : NetworkBehaviour {
    static Dictionary<string, LaunchDelegate> launches;
    static Pool<Projectile> pool = new();
    
    static Projectile Factory() {
        var go = ABMgr.Instance.LoadRes<GameObject>("entity_prefabs", "Projectile");
        return Instantiate(go).GetComponent<Projectile>();
    }
    public static void LoadLuaLaunchTable() {
        launches = XLuaVM.Instance.Global.Get<Dictionary<string, LaunchDelegate>>("Launch");
        pool.Warm(100,
            factory: Factory,
            onEnable: x => x.gameObject.SetActive(true),
            onRecycle: x => x.gameObject.SetActive(false));
    }
    public static Projectile Create(Vector2 position, Camp camp, int damage) {
        var obj = pool.Get();
        obj.transform.position = position;
        obj.camp = camp;
        obj.damage = damage;
        obj.liveTimer = obj.liveTime = 1.2f;
        return obj;
    }

    SpriteRenderer spriteRenderer;
    BoxCollider2D boxCld;
    Rigidbody2D rb;
    Camp camp;
    int damage;
    float liveTime;
    float liveTimer;
    public Rigidbody2D Rb => rb ??= GetComponent<Rigidbody2D>();

    public void Launch(Vector2 dir, string type) {
        launches[type](this, dir);
    }

    void RecycleSelf() {
        pool.Recycle(this);
    }
    private void OnTriggerEnter2D(Collider2D collision) {
        int layer = collision.gameObject.layer;
        if(layer == ConstData.BlockLayer)
            RecycleSelf();
        if (layer == ConstData.EnemyLayer && camp == Camp.Player)
            collision.GetComponent<FightEntity>().OnHit(damage);
    }
    private void Update() {
        liveTimer -= Time.deltaTime;
        if (liveTimer <= 0)
            RecycleSelf();
    }
}

public enum Camp {
    Player, Enemy
}