using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    //필요한 외부 데이터
    public MonsterData Data;
    public Transform player;
    public Transform groundCheck;
    public Transform wallCheck;
    public LayerMask groundLayer;

    [HideInInspector] public Rigidbody2D rb;
    private Animator anim;
    private float lastAttackTime;
    private int currentHp;

    private bool facingRight = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        currentHp = Data.MaxHp;
    }

    #region 이동 관련
    private void Update()
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
            rb.velocity = Vector2.zero;

    }
    public void MoveForwaard(Vector2 target, float speed)
    {
        float dir = target.x > transform.position.x ? 1 : -1;
        rb.velocity = new Vector2(dir * speed, rb.velocity.y);
    }

    public void MoveToWards(Vector2 target, float speed)
    {
        float dir = target.x > transform.position.x ? 1 : -1;
        if (dir > 0 && !facingRight) TurnAround();
        else if (dir < 0 && facingRight) TurnAround();

        rb.velocity = new Vector2(dir, rb.velocity.y);
    }
    public void Stop() => rb.velocity = new Vector2(0, rb.velocity.y);

    public void TurnAround()
    {
        facingRight = !facingRight;
        transform.localScale = new Vector3(facingRight ? 1 : -1, 1, 1);
    }
    #endregion

    #region 바닥 감지
    public bool IsGrounded()
    {
        if (groundCheck == null) return true;
        RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down * 1f, groundLayer);
        Debug.DrawRay(groundCheck.position, Vector2.down * 1f, hit ? Color.green : Color.red);
        return hit.collider != null;
    }
    public bool IsWallAhead()
    {
        if (wallCheck == null) return false;
        Vector2 dir = facingRight ? Vector2.right : Vector2.left;
        RaycastHit2D hit = Physics2D.Raycast(wallCheck.position, dir, 0.3f, groundLayer);
        Debug.DrawRay(wallCheck.position, dir * 0.3f, hit ? Color.yellow : Color.cyan);
        return hit.collider != null;
    }
    #endregion
}
