using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float jumpForce = 7.0f;
    [SerializeField] private float maxFallSpeed = 50f;

    [Header("GroundCheck")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("WallCheck")]
    [SerializeField] private Transform wallCheck;
    [SerializeField] private Vector2 wallCheckSize = new Vector2(0.5f, 1f);

    [Header("Dash")]
    [SerializeField] private float dashPower = 5.0f;
    [SerializeField] private float dashingTime = 0.2f;
    [SerializeField] private float dashCoolDown = 1f;

    //이동 관련
    private float inputX;
    private float inputY;
    private bool isGrounded;
    private bool jumpPressed;
    private bool isWalled;

    //대쉬 관련
    public bool unLockedDash;
    private bool isAbleDash = true;
    private bool isDashing;

    AnimationFSM.FSM fsm = new AnimationFSM.FSM();
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator anim;

    private bool isDamaged;
    private bool isAttacked;

    private void Awake()
    {

        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        fsm.AddState(new AnimationFSM.Attack1State("Attack1State"));
        fsm.AddState(new AnimationFSM.Attack2State("Attack2State"));
        fsm.AddState(new AnimationFSM.Attack3State("Attack3State"));
        fsm.AddState(new AnimationFSM.JumpState("JumpState"));
        fsm.AddState(new AnimationFSM.FallState("FallState"));
        fsm.AddState(new AnimationFSM.RunState("RunState"));
        fsm.AddState(new AnimationFSM.DashState("DashState"));
        fsm.AddState(new AnimationFSM.SlideState("SlideState"));
        fsm.AddState(new AnimationFSM.IdleState("IdleState"));
    }
    private void Update()
    {
        inputX = Input.GetAxisRaw("Horizontal");
        inputY = Input.GetAxisRaw("Vertical");

        if(Input.GetButton("Jump"))
        {
            jumpPressed = true;
        }

        Jump();
        OnDash();
        AnimationCondtion();
    }
    private void FixedUpdate()
    {
        //바닥 판정
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        Move();
        Flip();
        Attack();
    }
    //좌우 반전
    private void Flip()
    {
        if(inputX != 0)
        {
            if(inputX < 0)
            {
                spriteRenderer.flipX = true;
            }
            else
            {
                spriteRenderer.flipX=false;
            }
        }
    }
    //이동
    private void Move()
    {
        rb.velocity = new Vector2(inputX * moveSpeed , rb.velocity.y);

    }
    //점프
    private void Jump()
    {
        if(jumpPressed && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
        jumpPressed = false;
    }
    //적과 충돌 판정
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Enemy")
        {
            Debug.Log(collision.gameObject.tag + "와 부딪힘");
            OnDamaged(collision.transform.position);
        }
    }
    //피격시 무적판정
    private void OnDamaged(Vector2 targetPos)
    {
        //플레이어의 Layer를 변경
        gameObject.layer = 8;

        //피격시 투명하게 변경
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);
        //피격시 튕겨나가는 방향 결정
        int dirc = transform.position.x - targetPos.x > 0 ? 1 : -1;
        //좌우로 튕김
        rb.AddForce(new Vector2(dirc, 1) * 3, ForceMode2D.Impulse);

        //1초 후 무적 시간 끝
        Invoke("OffDamaged", 1); 
    }
    //무적시간 끝
    private void OffDamaged()
    {
        //플레이어 Layer 원상 복구
        gameObject.layer = 7;
        //색상 원상 복구
        spriteRenderer.color = new Color(1, 1, 1, 1f);
    }
    //대쉬 입력
    private void OnDash()
    {
        if (isDashing) { return; }

        if (unLockedDash)
        {
            if(Input.GetKeyDown(KeyCode.LeftShift) && isAbleDash)
            {
                StartCoroutine(Dash());
            }
        }
    }
    //대쉬 코루틴
    public IEnumerator Dash()
    {
        Debug.Log("Dash");
        Debug.Log("Dash Cool Time Start");

        isAbleDash = false;
        isDashing = true;
        //원본 중력값 백업
        var oringnalGravity = rb.gravityScale;
        //중력 = 0;
        rb.gravityScale = 0f;
        //잔상 만들기
        rb.velocity = new Vector2(transform.localScale.x * dashPower, 0f);
        yield return new WaitForSeconds(dashingTime);
        isDashing = false;
        //잔상 끝
        //중력 초기화
        rb.gravityScale = oringnalGravity;
        //행동 불가
        rb.velocity = new Vector2(0, 0);
        Debug.Log("중력 값" + rb.gravityScale);
        yield return new WaitForSeconds(dashCoolDown);
        isAbleDash = true;

        Debug.Log("Dash Cool Time Finished");
    }
    //공격
    private void Attack()
    {
        if(Input.GetKeyDown(KeyCode.K) && !isAttacked)
        {
            isAttacked = true;
            
        }
    }
    //애니메이션
    private void AnimationCondtion()
    {
        var animatorState = anim.GetCurrentAnimatorStateInfo(0);
        var velocity = rb.velocity;
        var sensitivity = 0.1f;

        //바닥 감지
        fsm.conditions.isGrounded = isGrounded;
        //x축 이동 감지
        fsm.conditions.moveDirection.x = (velocity.x > sensitivity ? 1 : 0) + (velocity.x < -sensitivity ? -1 : 0);
        //y축 이동 감지
        fsm.conditions.moveDirection.y = (velocity.y > sensitivity ? 1 : 0) + (velocity.y < -sensitivity ? -1 : 0);
        //대쉬 감지
        fsm.conditions.isDashing = isDashing;
        //공격 감지
        fsm.conditions.isAttacked = isAttacked;

        fsm.Update();

        var fsmAnimationName = fsm.currentState.animationName;
        //디버그
        if (!animatorState.IsName(fsmAnimationName))
        {
            anim.Play(fsmAnimationName);
        }
    }
    //죽음
    public void Die()
    {

    }
}
