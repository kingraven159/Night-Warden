using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class Monster : MonoBehaviour
{
    //필요한 외부 데이터
    public MonsterData Data;
    public PlayerData PlayerData;
    public Transform player;
    public LayerMask playerLayer;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Animator anim;
    private INode rootNode;
    private SpriteRenderer sr;

    private float lastAttackTime;
    private int currentHp;
    private bool isHit = false;
    private bool facingRight = true;
    private string currentAnim;
    public bool IsDead => currentHp <= 0;
    private Color originColor;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        currentHp = Data.MaxHp;
        originColor = sr.color;
    }

    private void Start()
    {
        BulidBehaviorTree();
    }
    private void Update()
    {
        if (IsDead) return;
        rootNode.Evaluate();
        DebugDrawRays();
    }
    private void BulidBehaviorTree()
    {
        // Attack 행동 노드 :플레이어 감지 -> 공격 가능 -> 공격
        var attackSquence = new SequenceNode(new List<INode>()
        {
            new ActionNode(CheckPlayerInAttackRange),
            new ActionNode(AttackPlayer)
        });

        // Chase 행동 노드 : 플레이어 감지 -> 추적
        var chaseSequence = new SequenceNode(new List<INode>()
        {
            new ActionNode(CheckPlayerDetected),
            new ActionNode(TurnToWardPlayer),
            new ActionNode(ChasePlayer)
        });

        // Patrol 행동 노드
        var patrolNode = new ActionNode(Patrol);

        // 최종 트리 : Attack -> Chase -> Patrol
        rootNode = new SelectorNode(new List<INode>()
        {
            attackSquence,
            chaseSequence,
            patrolNode
        });
    }

    #region 행동 노드 함수들
    private INode.ENodeState CheckPlayerDetected()
    {
        if (player && Vector2.Distance(transform.position, player.position) <= Data.DetectionRange)
            return INode.ENodeState.Success;
        return INode.ENodeState.Failure;
    }
    private INode.ENodeState CheckPlayerInAttackRange()
    {
        if (player && Vector2.Distance(transform.position, player.position) <= Data.AttackRange)
            return INode.ENodeState.Success;
        return INode.ENodeState.Failure;
    }
    private INode.ENodeState Patrol()
    {
        SetAnimation("Walk");

        MoveForward(Data.MoveSpeed);
        return INode.ENodeState.Running;
    }
    private INode.ENodeState ChasePlayer()
    {
        SetAnimation("Walk");
        MoveForward(Data.ChaseSpeed);
        return INode.ENodeState.Running;
    }
    private INode.ENodeState TurnToWardPlayer()
    {
        float dir = player.position.x - transform.position.x;
        if ((dir > 0 && !facingRight) || (dir < 0 && facingRight))
            TurnAround();
        return INode.ENodeState.Success;
    }
    private INode.ENodeState AttackPlayer()
    {
        if (Time.time - lastAttackTime < Data.AttackCooldown)
            return INode.ENodeState.Failure;

        SetAnimation("Attack");
        rb.velocity = Vector2.zero;
        lastAttackTime = Time.time;

        Collider2D[] hitPlayer = Physics2D.OverlapCircleAll(transform.position, Data.AttackRange, playerLayer);
        foreach (var hit in hitPlayer)
        {
            //플레이어 데미지 주기
            PlayerData.currentHp -= Data.Damage;
            Debug.Log($"{hit.name}에게 {Data.Damage}만큼 데미지 주기");
        }
        return INode.ENodeState.Success;
    }
    #endregion

    #region 이동 관련
    public void MoveForward(float speed)
    {
        float dir = facingRight ? 1 : -1;
        rb.velocity = new Vector2(dir * speed, rb.velocity.y);

        //바닥이 없거나 벽이 있으면 반전
        if (!IsGroundAhead() || IsWallAhead())
        {
            TurnAround();
        }
    }
    public void TurnAround()
    {
        facingRight = !facingRight;
        transform.localScale = new Vector3(facingRight ? 1 : -1, 1, 1);
    }
    #endregion

    #region 감지 관련
    private Vector2 GetFrontVec()
    {
        float offest = 0.4f;
        return new Vector2(rb.position.x + (facingRight ? offest : -offest), rb.position.y);
    }
    public bool IsGroundAhead()
    {
        Vector2 frontVec = GetFrontVec();
        RaycastHit2D hit = Physics2D.Raycast(frontVec, Vector2.down, 1.2f, groundLayer);

        return hit.collider != null;
    }
    public bool IsWallAhead()
    {
        Vector2 frontVec = GetFrontVec();
        Vector2 dir = facingRight ? Vector2.right : Vector2.left;
        RaycastHit2D hit = Physics2D.Raycast(frontVec, dir, 0.5f, groundLayer);

        return hit.collider != null;
    }
    private void DebugDrawRays()
    {
        Vector2 frontVec = GetFrontVec();
        Debug.DrawRay(frontVec, Vector2.down * 1.2f, Color.green);
        Vector2 dir = facingRight ? Vector2.right : Vector2.left;
        Debug.DrawRay(frontVec, dir * 0.6f, Color.red);
    }
    private void OnDrawGizmosSelected()
    {

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, Data.AttackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, Data.DetectionRange);
    }
    #endregion

    #region 피격 시 깜빡임 효과
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("PlayerWeapon"))
        {
            TakeDamage(PlayerData.atk);
        }      
    }
    public void TakeDamage(int damage)
    {
        currentHp -= damage;
        Debug.Log($"{gameObject.name} 피격, 체력 : {currentHp}");

        if (currentHp > 0)
        {
            StartBlink();
            SetAnimation("Hurt");
            isHit = true;
            rb.velocity = Vector2.zero;

            Vector2 knockDir = (player.position.x < transform.position.x) ? Vector2.right : Vector2.left;
            rb.AddForce(knockDir * 1f + Vector2.up * 1f, ForceMode2D.Impulse);

            Invoke(nameof(RecoverFromHit), 0.5f);
        }
        else
        {
            Die();
        }
    }
    private void RecoverFromHit()
    {
        StopBlink();
        isHit = false;
    }
    private void StartBlink()
    {
        CancelInvoke(nameof(Blink));
        InvokeRepeating(nameof(Blink), 0f, 0.1f);
    }
    private void StopBlink()
    {
        CancelInvoke(nameof(Blink));
        sr.color = originColor;
    }
    private void Blink()
    {
        Color c = sr.color;
        c.a = c.a == 1f ? 0.3f : 1f;
        sr.color = c;
    }
    #endregion

    #region 기타
    private void SetAnimation(string stateName)
    {
        if (anim == null) return;
        if (currentAnim == stateName) return; // 이미 같은 애니메이션이면 무시

        currentAnim = stateName;
        anim.Play(stateName);
    }
    private void Die()
    {
        StopBlink();
        rb.velocity = Vector2.zero;
        StopBlink();
        DropCoins();
        Destroy(gameObject, 0.5f);
    }
    private void DropCoins()
    {
        if(Data.CoinPrefab == null) return;

        for(int i = 0; i < Data.DropCount; i++)
        {
            Vector3 spawnPos = transform.position + new Vector3(Random.Range(-0.5f, 0.5f), 0.5f, 0);
            Instantiate(Data.CoinPrefab, spawnPos, Quaternion.identity);
        }
    }
    #endregion
}


