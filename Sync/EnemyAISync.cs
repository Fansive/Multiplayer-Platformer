using Unity.Netcode;
using System.Linq;
using UnityEngine;

internal class EnemyAISync : NetworkBehaviour {
    public Transform Player;
    public float Speed;
    public float JumpForce;

    const float RandomMoveTime = 10f;
    NetworkVariable<Vector3> SyncedPos = new();
    float randMoveTimer = float.NaN;
    BoxCollider2D cld;
    Rigidbody2D rb;
    Animator animator;
    AIState aiState;
    int curDir;
    bool isGround, isFrontGround, isFrontWall;
    bool isJumpReady = true;
    Vector2 targetPos => PlayerInfo.PlayerList.Values.MinBy(x => 
        Vector2.Distance(x.transform.position, transform.position)).transform.position;
    bool isHorizontalClose => Mathf.Abs(transform.position.x - targetPos.x) < 8f;
    bool isVerticalClose => Mathf.Abs(transform.position.y - targetPos.y) < 3f;
    int targetDir => targetPos.x > transform.position.x ? 1 : -1;
    Vector2 upperEdge; //该点是向上跳上实心块平台时的边界点
    BlockType GetBlockTypeInCell(int x, int y) {
        var solid = GameRoot.Instance.SolidTilemap;
        if (solid.cellBounds.Contains(new(x, y)) && solid.GetTile(new(x, y)) != null)
            return BlockType.Solid;
        var platform = GameRoot.Instance.PlatformTilemap;
        if (platform.cellBounds.Contains(new(x, y)) && platform.GetTile(new(x, y)) != null)
            return BlockType.Platform;
        return BlockType.Air;
    }
    void Start() {
        cld = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        rb.sharedMaterial = new PhysicsMaterial2D() { friction = 0 };
        transform.position = new(Random.value * 5, Random.value * 5);
    }
    void UpdateCurDir() {
        if (aiState is not (AIState.RandomMoveUp or AIState.RandomMoveDown)) {
            curDir = targetDir;
        }
    }
    void Move() {
        rb.velocity = new(curDir * Speed, rb.velocity.y);

    }
    void DetectBlock() {
        isGround = Util.CheckBlock(cld, Vector2.zero, Vector2.down, 0.1f, true);
        Vector2 frontOffset = new(curDir * Speed * Time.fixedDeltaTime, 0);
        isFrontGround = Util.CheckBlock(cld, frontOffset, Vector2.down, 0.1f, true);
        isFrontWall = Util.CheckBlock(cld, frontOffset, Vector2.up, 0.1f, true);
    }
    bool Jump() {
        if (isGround && isJumpReady) {
            rb.velocity = new(rb.velocity.x, 0);
            rb.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);
            isJumpReady = false;
            Paltry.DelayCall.Call(0.1f, () => isJumpReady = true);
            return true;
        }
        return false;
    }
    void TryJumpToUpperEdge() {
        float curX = transform.position.x, curY = transform.position.y;
        var jumpPoint = Util.CalcJumpTopPoint(Speed * -curDir, JumpForce, rb.mass);
        if(curDir == 1 && curY + jumpPoint.y > upperEdge.y + 1 && curX > upperEdge.x - 1 - jumpPoint.x) {
            if (Jump()) {
                curDir = -curDir;
                aiState = AIState.Far;
            }
        }
        else if(curDir == -1 && curY + jumpPoint.y > upperEdge.y + 1 && curX < upperEdge.x + 1 - jumpPoint.x) {
            if (Jump()) {
                curDir = -curDir;
                aiState = AIState.Far;
            }
        }
    }
    void TryJumpToAttack() {
        //TODO 可以改为,当玩家碰撞盒下沿高于自身碰撞盒上沿时才起跳
        if (targetPos.y > transform.position.y)
            Jump();
    }
    BlockType FindVerticalBlockType(bool isUpward, out int closestSolidY) {
        var curCell = GameRoot.Instance.SolidTilemap.WorldToCell(transform.position);
        int end = GameRoot.Instance.SolidTilemap.WorldToCell(targetPos).y;
        int increment = isUpward ? 1 : -1;
        closestSolidY = int.MinValue;
        for (int y = curCell.y; y != end; y += increment) {
            switch (GetBlockTypeInCell(curCell.x, y)) {
                case BlockType.Solid:
                    closestSolidY = y;
                    return BlockType.Solid;
                case BlockType.Platform:
                    return BlockType.Platform;
            }
        }
        return BlockType.Air;
    }
    
    void RandomMoveUp(int closestSolidY) {
        aiState = AIState.RandomMoveUp;
        randMoveTimer = RandomMoveTime;
        curDir = Random.value >= 0.5f ? 1 : -1;
        int curX = GameRoot.Instance.SolidTilemap.WorldToCell(transform.position).x;
        int y = closestSolidY, playerY = GameRoot.Instance.SolidTilemap.WorldToCell(targetPos).y;
        int farthestX = curX, farthestY = y;
        while (y != playerY && GetBlockTypeInCell(curX, y) == BlockType.Solid) {
            int x = curX;
            while (GetBlockTypeInCell(x, y) == BlockType.Solid)
                x += curDir;
            x -= curDir;
            if (curDir == 1 && x > farthestX) {
                farthestX = x;
                farthestY = y;
            }
            else if(curDir == -1 && x < farthestX) {
                farthestX = x;
                farthestY = y;
            }
            y++;
        }
        upperEdge = GameRoot.Instance.SolidTilemap.CellToWorld(new(farthestX, farthestY));
    }
    void RandomMoveDown() {
        aiState = AIState.RandomMoveDown;
        randMoveTimer = RandomMoveTime;
    }
    void JumpDownPlatform() {

    }
    void ResetAI() {
        aiState = AIState.Far;

    }
    void FixedUpdate() {
        if (!IsServer) {
            transform.position = SyncedPos.Value;
            return;
        }
        UpdateCurDir();
        DetectBlock();
        if (!isHorizontalClose && aiState is not (AIState.RandomMoveUp or AIState.RandomMoveDown)) {
            aiState = AIState.Far;
        }
        else {
            if (isVerticalClose) {
                Move();
                TryJumpToAttack();
            }
            if (aiState is AIState.RandomMoveUp or AIState.RandomMoveDown) {

            }
            else if (targetPos.y > transform.position.y) {
                switch (FindVerticalBlockType(true, out int closestSolidY)) {
                    case BlockType.Solid: RandomMoveUp(closestSolidY); break;
                    case BlockType.Platform:
                    case BlockType.Air: Move(); TryJumpToAttack(); break;
                }
            }
            else {
                switch (FindVerticalBlockType(false, out int closestSolidY)) {
                    case BlockType.Solid: RandomMoveDown(); break;
                    case BlockType.Platform: Move(); JumpDownPlatform(); break;
                    case BlockType.Air: Move(); TryJumpToAttack(); break;
                }
            }
        }
        switch(aiState) {
            case AIState.Far:
                Move();
                if (!isFrontGround || isFrontWall)
                    Jump();
                break;
            case AIState.RandomMoveUp:
                TryJumpToUpperEdge();
                Move();
                break;
            case AIState.RandomMoveDown:
                Move();
                if(isFrontGround) 
                    Jump();
                break;
        }
    }
    void Update() {
        animator.SetFloat("MoveX", curDir);
        if (!IsServer) {
            transform.position = SyncedPos.Value;
            return;
        }
        if (aiState is AIState.RandomMoveUp or AIState.RandomMoveDown) {
            randMoveTimer -= Time.deltaTime;
            if(randMoveTimer <= 0) {
                ResetAI();
            }
        }

        SyncPosition();
        //print($"<{aiState}> Dir:{curDir}");
    }
    void SyncPosition() {
        SyncedPos.Value = transform.position;
    }
    enum AIState {
        Far, RandomMoveUp, RandomMoveDown,
    }
    enum BlockType {
        Air, Solid, Platform
    }
}
