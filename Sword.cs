using System.Collections.Generic;
using System;
using System.Collections;
using UnityEngine;

internal class Sword : MonoBehaviour {
    public int Damage;
    public float AttackInterval;

    [SerializeField] SpriteRenderer swordSR;
    BoxCollider2D cld;
    HashSet<GameObject> collidedSet = new();
    float attackTimer;
    bool isSwinging;

    private void Start() {
        cld = GetComponent<BoxCollider2D>();
        EnableSword(false);
        attackTimer = 0;
    }
    public void EnableSword(bool isActive) {
        swordSR.enabled = cld.enabled = isActive;
    }
    public void Swing(float time, int dir) {
        if (attackTimer > 0 || isSwinging)
            return;
        attackTimer = AttackInterval;
        StartCoroutine(Swing_Core(time, dir));
    }
    IEnumerator Swing_Core(float time, int dir) {
        isSwinging = true;
        EnableSword(true);
        collidedSet.Clear();
        float delta = 90f / (time / Time.fixedDeltaTime);
        float angle = 90f;
        if(dir == 1) {
            while (angle > 0) {
                transform.rotation = Quaternion.Euler(0, 0, angle);
                angle -= delta;
                yield return new WaitForFixedUpdate();
            }
        }
        else if(dir == -1) {
            while (angle < 180) {
                transform.rotation = Quaternion.Euler(0, 0, angle);
                angle += delta;
                yield return new WaitForFixedUpdate();
            }
        }
        EnableSword(false);
        isSwinging = false;
    }
    private void Update() {
        if(attackTimer > 0)
            attackTimer -= Time.deltaTime;
        
    }
    void OnTriggerEnter2D(Collider2D collision) {
        var go = collision.gameObject;
        if (collidedSet.Contains(go))
            return;
        collidedSet.Add(go);
        go.GetComponent<FightEntity>().OnHit(Damage);
    }
 
}
