using Unity.Netcode;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

internal class PlayerControlSync : NetworkBehaviour {
    public float GroundFriction = 0f, AirFriction = 0f; //将摩擦力设为0可以避免出现在地面上走的比在空中慢的情况
    public float MaxSpeed = 5f;
    public float MoveForce = 16f, JumpForce = 18f;

    [SerializeField] Sword sword;
    NetworkVariable<Vector3> SyncedPos =
        new(writePerm: NetworkVariableWritePermission.Owner);
    NetworkVariable<float> inputH = 
        new(writePerm: NetworkVariableWritePermission.Owner);
    PlayerInfo playerInfo;
    Rigidbody2D rb;
    BoxCollider2D cld;
    Animator animator;
    bool isGround;
    bool isJumpKeyDown = false;
    int currentItemId = 1;
    private void Start() {
        playerInfo = GetComponent<PlayerInfo>();
        rb = GetComponent<Rigidbody2D>();
        cld = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
        rb.sharedMaterial = new PhysicsMaterial2D() { friction = GroundFriction, bounciness = 0 };
    }
    void FixedUpdate() {
        if (!IsOwner)
            return;
        DetectGround();
        rb.sharedMaterial.friction = isGround ? GroundFriction : AirFriction;
        Move();
        Jump();
        ClampSpeed();
    }
    void Update() {
        SetAnim();
        if (!IsOwner) {
            transform.position = SyncedPos.Value;
            return;
        }

        GetInput();
        
        SyncPosition();
    }

    void GetInput() {
        inputH.Value = Input.GetAxisRaw("Horizontal");
        if(Input.GetKeyDown(KeyCode.Space)) 
            isJumpKeyDown = true;
        if(Input.GetKeyDown(KeyCode.Mouse1)) {
            var hit = Physics2D.Raycast(Util.MousePosInWorld(), Vector2.one, 0.01f, ConstData.InteractableLayerMask);
            if (hit.collider != null) {
                hit.transform.GetComponent<Interactable>().OnInteract(playerInfo);
            }
        }
        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            UseCurrentItem();
        }
        if (Input.GetKeyDown(KeyCode.Alpha1)) currentItemId = 1;
        else if(Input.GetKeyDown(KeyCode.Alpha2)) currentItemId = 2;
    }
    void UseCurrentItem() {
        switch (currentItemId) {
            case 1: sword.Swing(sword.AttackInterval, (int)animator.GetFloat("Dir"));
                break;
            case 2: var proj = Projectile.Create(transform.position, Camp.Player, 5);
                Vector2 dir = Util.MousePosInWorld() - transform.position;
                proj.Launch(dir.normalized, "laser");
                break;
        }

    }
    void SetAnim() {
        float v = inputH.Value;
        if (v != 0)
            animator.SetFloat("Dir", Math.Sign(v));
        animator.SetBool("IsMoving", v != 0);
    }
    void DetectGround() {
        isGround = Util.CheckBlock(cld, Vector2.zero, Vector2.down, 0.1f, true);
    }
    void Move() {
        Vector2 dir = inputH.Value * Vector2.right;
        rb.AddForce(dir * MoveForce);
    }
    void Jump() {
        if (isJumpKeyDown && isGround) {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);
            isJumpKeyDown = false;
        }
    }
    void ClampSpeed() {
        float newVelocityX = Mathf.Clamp(rb.velocity.x, -MaxSpeed, MaxSpeed);
        rb.velocity = new(newVelocityX, rb.velocity.y);
    }
    void SyncPosition() {
        SyncedPos.Value = transform.position;
    }
}


