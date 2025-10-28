using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using System;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;

public class PlayerController : MonoBehaviour
{
    public PlayerData Data;

    //상태
    public bool IsFacingRight {  get; private set; }
    public bool IsJumping { get; private set; }
    public bool IsWallJumping { get; private set; }
    public bool IsDashing { get; private set; }
    public bool IsSliding { get; private set; }

    //타이머
    public float LastOnGroundTime {  get; private set; }
    public float LastOnWallTime { get; private set; }
    public float LastOnWallRightTime { get; private set; }
    public float LastOnWallLeftTime { get; private set; }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5.0f;

    [Header("GroundCheck")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("WallCheck")]
    [SerializeField] private Transform frontWallCheckPoint;
    [SerializeField] private Transform backWallCheckPoint;
    [SerializeField] private Vector2 wallCheckSize = new Vector2(0.5f, 1f);

    //마지막에 입력한 시간
    public float LastPressedJumpTime { get; private set; }
    public float LastPressedDashTime { get; private set; }  

    //이동 관련
    private float inputX;
    private float inputY;
    private bool isGrounded;

    //점프 관련
    private bool isFalling;
    private bool isJumpCut;

    //벽점프 관련
    private float wallJumpStartTime;
    private bool isWallJumping;

    //대쉬 관련
    public bool unLockedDash;
    private int dashesCount;
    private bool dashRefilling;
    private Vector2 lastDashDir;
    private bool isDashAttacking;

    //전투 관련
    private bool isDamaged;
    private bool isAttacked;

    AnimationFSM.FSM fsm = new AnimationFSM.FSM();
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator anim;

    private void Awake()
    {

        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        //애니메이션 스테이트
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
    private void Start()
    {
        //중력 값 세팅
        SetGravityScale(Data.gravityScale);
        IsFacingRight = true;
    }
    private void Update()
    {
        //타이머
        LastOnGroundTime -= Time.deltaTime;
        LastOnWallTime -= Time.deltaTime;
        LastOnWallRightTime -= Time.deltaTime;
        LastOnWallLeftTime -= Time.deltaTime;

        LastPressedJumpTime -= Time.deltaTime;
        LastPressedDashTime -= Time.deltaTime;


        inputX = Input.GetAxisRaw("Horizontal");
        inputY = Input.GetAxisRaw("Vertical");

        if(Input.GetButtonDown("Jump"))
        {
            OnJumpInput();
        }

        if(Input.GetButtonUp("Jump"))
        {
            OnJumpUpInput();
        }
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            OnDashInput();
        }

        if (!IsDashing && !IsJumping)
        {
            if (isGrounded)
            {
                if(LastOnGroundTime < -0.1f)
                {
                    AnimationFSM.IdleState = true; 
                }
                LastOnGroundTime = Data.coyoteTime;
            }
        }

        Jump();
        AnimationCondtion();
    }
    private void FixedUpdate()
    {
        //바닥 판정
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        //벽 판정
        IsFacingRight = Physics2D.OverlapBox(frontWallCheckPoint.position, wallCheckSize, 0, groundLayer) || Physics2D.OverlapBox(backWallCheckPoint.position, wallCheckSize, 0, groundLayer);

        //점프를 하지 않고 바닥에 붙어있을시 코요테 타임
        if(!IsJumping && isGrounded)  
            LastOnGroundTime = Data.coyoteTime;        
        if (IsFacingRight)
            LastOnGroundTime = Data.coyoteTime;

        //벽 점프 중일때
        if (isWallJumping)
            Run(Data.wallJumpRunLerp);      
        else
            Run(1);
        
        //벽 슬라이딩 
        if (IsSliding)
            Slide();

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
    private void Run(float lerpAmount)
    {
        //이동속도
        float runSpeed = moveSpeed * Data.runMaxSpeed;
        //이독속도 스케일
        runSpeed = Mathf.Lerp(rb.velocity.x, runSpeed, lerpAmount);
        //가속도 딜레이
        float accelRate;

        //지상에 있을때
        if (LastOnGroundTime > 0)
            //이동속도가 0.01f 보다 빠를때 가속하고 아니면 감속
            accelRate = (Mathf.Abs(runSpeed) > 0.01f) ? Data.runAccelAmount : Data.runDeccelAmount;
        else
            //이동속도가 0.01f 보다 빠를때 가속하고 아니면 감속 
            accelRate = (Mathf.Abs(runSpeed) > 0.01f) ? Data.runAccelAmount * Data.accelInAir : Data.runDeccelAmount * Data.deccelInAir;
        
        //점프나 추락중일때 
        if((IsJumping || isWallJumping || isFalling) && Mathf.Abs(rb.velocity.y) < Data.jumpHangTimeThreshold)
        {
            accelRate *= Data.jumpHangAccelerationMult;
            runSpeed *= Data.jumpHangMaxSpeedMult;
        }

        if(Data.doConserveMomantum && Mathf.Abs(rb.velocity.x) > Mathf.Abs(runSpeed) && Mathf.Sign(rb.velocity.x) == Mathf.Sign(runSpeed) && Mathf.Abs(runSpeed) > 0.01f && LastOnGroundTime < 0)
        {
            accelRate = 0;
        }

        float speedDif = runSpeed - rb.velocity.x;
        float movement = speedDif * accelRate;

        rb.AddForce(movement * Vector2.right, ForceMode2D.Impulse);

    }
    //점프 인풋
    private void OnJumpInput()
    {
        //버퍼링 
        LastPressedJumpTime = Data.jumpInputBufferTime;
    }
    //이중 점프 인풋
    private void OnJumpUpInput()
    {
        if (CanJumpCut() || CanWallJumpCut())
            isJumpCut = true;
    }
    //대쉬 인풋
    private void OnDashInput()
    {
        //버퍼링
        LastPressedDashTime = Data.dashIputBufferTime;
    }
    //점프
    private void Jump()
    {
        LastPressedJumpTime = 0;
        LastOnGroundTime = 0;

        float jumpForce = Data.jumpForce;

        if(rb.velocity.y < 0)
            jumpForce -= rb.velocity.y;
        
        IsJumping = true;
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        
        IsJumping=false;
    }
    //벽 점프
    private void WallJump(int dir)
    {
        
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
    //피격시 밀쳐짐 & 무적판정
    private void OnDamaged(Vector2 targetPos)
    {
        isDamaged = true;
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
        isDamaged = false;
    }
    //대쉬 코루틴
    public IEnumerator StartDash(Vector2 dir)
    {
        LastOnGroundTime = 0;
        LastPressedDashTime = 0;

        Debug.Log("Dash");
        Debug.Log("Dash Cool Time Start");

        float startTime = Time.time;

        dashesCount--;
        //대쉬 공격 활성화
        isDashAttacking = true;

        //무중력
        SetGravityScale(0);

        //대쉬공격이 가능한 시간
        while (Time.time - startTime <= Data.dashAttackTime)
        {
            rb.velocity = dir.normalized * Data.dashSpeed;
            yield return null;
        }

        startTime = Time.time;
        isDashAttacking = false;

        //중력스케일 복구
        SetGravityScale(Data.gravityScale);
        //대쉬 속도 감소
        rb.velocity = Data.dashEndSpeed * dir.normalized;

        //대쉬가 끝날때 까지의 시간
        while (Time.time - startTime <= Data.dashEndTime)
        {
            yield return null;
        }

        //대쉬 끝
        IsDashing = false;

        Debug.Log("Dash Cool Time Finished");
    }
    //대쉬 리필
    private IEnumerator RefillDash(int amount)
    {
        //대쉬 리필 중
        dashRefilling = true;
        yield return new WaitForSeconds(Data.dashRefillTime);
        //대쉬 리필 끝
        dashRefilling = false;
        dashesCount = Mathf.Min(Data.dashAmount, dashesCount + 1);
    }
    //벽 슬라이드
    private void Slide()
    {
        // y축 속도가 0보다 크면 
        if(rb.velocity.y >0)
        {
            // 
            rb.AddForce(rb.velocity.y * Vector2.up, ForceMode2D.Impulse);
        }

        float speedDif = Data.slideSpeed - rb.velocity.y;
        float movement = speedDif * Data.slideAccel;

        movement = Mathf.Clamp(movement, -Math.Abs(speedDif) * (1 / Time.fixedDeltaTime), Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime));

        rb.AddForce(movement * Vector2.up);
    }
    //공격
    private void Attack()
    {
        if(Input.GetKeyDown(KeyCode.K) && !isAttacked)
        {
            isAttacked = true;
            
        }
    }
    //중력 스케일 세팅
    public void SetGravityScale(float gravityScale)
    {
        rb.gravityScale = gravityScale;
    }
    //애니메이션 컨티션 값 입력
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
        fsm.conditions.isDashing = IsDashing;
        //공격 감지
        fsm.conditions.isAttacked = isAttacked;
        //피격 감지
        fsm.conditions.isDamaged = isDamaged;

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
    private void CheckDirectionToFace(bool isMovingRight)
    {
        if (isMovingRight != IsFacingRight)
            Flip();
    }
    //점프 조건
    private bool CanJump()
    {
        return LastPressedJumpTime > 0 && !IsJumping;
    }
    //이중 점프 조건
    private bool CanJumpCut()
    {
        return IsJumping && rb.velocity.y > 0;
    }
    //벽 점프 컷 조건
    private bool CanWallJumpCut()
    {
        return isWallJumping && rb.velocity.y > 0;
    }
    //대쉬 조건
    private bool CanDash()
    {
        if(!IsDashing && dashesCount < Data.dashAmount && LastOnGroundTime > 0 && !dashRefilling)
        {
            StartCoroutine(nameof(RefillDash), 1);
        }
        return dashesCount > 0;
    }
    //벽 슬라이드 조건
    private bool CanSlide()
    {
        if (LastOnWallTime > 0 && !IsJumping && !IsWallJumping && !IsDashing && LastOnGroundTime <=0)
            return true;
        else
            return false;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(frontWallCheckPoint.position, wallCheckSize);
        Gizmos.DrawWireCube(backWallCheckPoint.position, wallCheckSize);
    }
}
