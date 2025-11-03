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
    public bool IsAttachedWall {  get; private set; }
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
    public float comboTimer { get; private set; }

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
    public float LastPressedAttackTime { get; private set; }

    //이동 관련
    private Vector2 moveInput;
    private bool isGrounded;

    //점프 관련
    private int curJumpCount;
    private bool isFalling;
    private bool isJumpCut;

    //벽점프 관련
    private float wallJumpStartTime;
    private int lastWallJumpDir;

    //대쉬 관련
    private int dashesCount;
    private bool dashRefilling;
    private Vector2 lastDashDir;
    private bool isDashAttacking;

    //전투 관련
    private bool isDamaged;
    private bool isAttacked;
    private int currentAttackIndex = 0;
    private float comboResetTime = 10;

    AnimationFSM.FSM fsm = new();
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator anim;

    private void Awake()
    {

        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        //애니메이션 스테이트
        fsm.AddState(new AnimationFSM.DieState("DieState"));
        fsm.AddState(new AnimationFSM.HurtState("HurtState"));
        fsm.AddState(new AnimationFSM.Attack1State("Attack1State"));
        fsm.AddState(new AnimationFSM.Attack2State("Attack2State"));
        fsm.AddState(new AnimationFSM.Attack3State("Attack3State"));
        fsm.AddState(new AnimationFSM.AttackUpState("AttackUpState"));
        fsm.AddState(new AnimationFSM.AttackDownState("AttackDownState"));
        fsm.AddState(new AnimationFSM.AttackRunState("AttackRunState"));
        fsm.AddState(new AnimationFSM.WallSlideState("WallSlideState"));
        fsm.AddState(new AnimationFSM.FallState("FallState"));
        fsm.AddState(new AnimationFSM.JumpState("JumpState"));
        fsm.AddState(new AnimationFSM.RunState("RunState"));
        fsm.AddState(new AnimationFSM.Run2State("Run2State"));
        fsm.AddState(new AnimationFSM.DashState("DashState"));
        fsm.AddState(new AnimationFSM.SlideState("SlideState"));
        fsm.AddState(new AnimationFSM.SwordIdleState("SwordIdleState"));
        fsm.AddState(new AnimationFSM.IdleState("IdleState"));
    }
    private void Start()
    {
        //중력 값 세팅
        SetGravityScale(Data.gravityScale);
        IsFacingRight = true;
        curJumpCount = Data.jumpCount;
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
        LastPressedAttackTime -= Time.deltaTime;

        //이동 입력
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        
        //좌우 반전
        if (moveInput.x != 0)
            CheckDirectionToFace(moveInput.x > 0);
        //Input
        if (!isDamaged)
        {
            //점프
            if (Input.GetButtonDown("Jump"))
            {
                OnJumpInput();
            }
            //이중 점프
            if (Input.GetButtonUp("Jump"))
            {
                OnJumpUpInput();
            }
            //대쉬 공격
            if(Input.GetKeyDown(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.K))
            {
                OnDashInput();
                DashAttack();
            }
            //대쉬
            else if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                OnDashInput();
            }
            //공격
            else if (Input.GetKeyDown(KeyCode.K))
            {
                OnAttackInput();
            }
        }
        if (!IsDashing && !IsJumping)
        {
            //바닥에 있을때 점프와 코요테 타임 리셋
            if (isGrounded)
            {
                curJumpCount = Data.jumpCount;
                LastOnGroundTime = Data.coyoteTime;
            }

            //오른쪽 벽 체크
            if (((Physics2D.OverlapBox(frontWallCheckPoint.position, wallCheckSize, 0, groundLayer) && IsFacingRight) ||
                (Physics2D.OverlapBox(backWallCheckPoint.position, wallCheckSize, 0, groundLayer) && !IsFacingRight)) && !IsWallJumping)
            {
                LastOnWallRightTime = Data.coyoteTime;
                Debug.Log("오른벽 코요테" + LastOnWallRightTime);
            }
            //왼쪽 벽 체크
            if (((Physics2D.OverlapBox(frontWallCheckPoint.position, wallCheckSize, 0, groundLayer) && !IsFacingRight) ||
                (Physics2D.OverlapBox(backWallCheckPoint.position, wallCheckSize, 0, groundLayer) && IsFacingRight)) && !IsWallJumping)
            {
                LastOnWallLeftTime = Data.coyoteTime;
                Debug.Log("왼벽 코요테" + LastOnWallLeftTime);
            }
            //벽 체크
            LastOnWallTime = Mathf.Max(LastOnWallLeftTime, LastOnWallRightTime);
        }
        if(IsJumping && rb.velocity.y <0)
        {
            IsJumping = false;
            isFalling = true;
        }
        if(IsWallJumping && Time.time - wallJumpStartTime > Data.wallJumpTime)
        {
            IsWallJumping = false;
        }

        if(LastOnGroundTime> 0 && !IsJumping && !IsWallJumping)
        {
            isJumpCut = false;
            isFalling = false;
        }

        if(!IsDashing)
        {
            if(CanJump() && LastPressedJumpTime > 0)
            {
                IsJumping = true;
                IsWallJumping = false;
                isJumpCut = false;
                isFalling = false;
                Jump();
            }
            else if(CanWallJump() && LastPressedJumpTime > 0)
            {
                IsWallJumping = true;
                IsJumping = false;
                isJumpCut = false;
                isFalling = false;

                wallJumpStartTime = Time.time;
                lastWallJumpDir = (LastOnWallRightTime > 0) ? -1 : 1;

                WallJump(lastWallJumpDir);
            }
        }

        if (CanDash() && LastPressedDashTime > 0)
        {
            Freeze(Data.dashFreezeTime);

            if (moveInput != Vector2.zero)
                lastDashDir = moveInput;
            else
                lastDashDir = IsFacingRight ? Vector2.right : Vector2.left;

            IsDashing = true;
            IsJumping = false;
            IsWallJumping = false;
            isJumpCut = false;

            StartCoroutine(nameof(StartDash), lastDashDir);
        }

        if (CanSlide() && ((LastOnWallLeftTime > 0 && moveInput.x < 0) || (LastOnWallRightTime > 0 && moveInput.x > 0)))
        {
            IsSliding = true;
            curJumpCount = Data.jumpCount;
        }
        else
            IsSliding = false;

        if (!isDashAttacking)
        {
            //높은데서 떨어질때 
            if (IsSliding)
            {
                SetGravityScale(0);
            }
            else if (rb.velocity.y < 0 && moveInput.y < 0)
            {
                //중력 스케일을 더 크게 적용
                SetGravityScale(Data.gravityScale * Data.fastFallGravityMult);
                //떨어지는 최고 속도 
                rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -Data.maxFastFallSpeed));
            }
            else if (isJumpCut)
            {
                SetGravityScale(Data.gravityScale * Data.jumpCutGravityMult);
                rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -Data.maxFastFallSpeed));
            }
            else if ((IsJumping || IsWallJumping || isFalling) && Mathf.Abs(rb.velocity.y) < Data.jumpHangTimeThreshold)
            {
                SetGravityScale(Data.gravityScale * Data.jumpHangGravityMult);
            }
            else if(rb.velocity.y < 0)
            {
                SetGravityScale(Data.gravityScale);

                rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -Data.maxFallSpeed));
            }
            else
            {
                SetGravityScale(Data.gravityScale);
            }
        }
        else
        {
            SetGravityScale(0);
        }

        if (comboTimer > 0)
            comboTimer -= Time.deltaTime;
        else
            OnComboEnd();

        if(Data.currentHp <= 0)
        {
            Died();
        }

        UpdateConditions();
    }
    private void FixedUpdate()
    {
        //바닥 판정
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        //벽 판정
        IsAttachedWall = Physics2D.OverlapBox(frontWallCheckPoint.position, wallCheckSize, 0, groundLayer) || Physics2D.OverlapBox(backWallCheckPoint.position, wallCheckSize, 0, groundLayer);

        //대쉬 이동, 피격시 이동 제한
        if (!IsDashing && !isDamaged && !isAttacked)
        {
            //벽 점프 중일때
            if (IsWallJumping)
                Run(Data.wallJumpRunLerp);
            else
                Run(1f);
        }
        else if (isDashAttacking)
        {
            Debug.Log("대쉬공격");
            Run(Data.dashEndRunLerp);
        }

        //벽 슬라이딩 
        if (IsSliding)
            Slide();
        
        if(CanAttack() && LastPressedAttackTime > 0)
        {
            Attack();
        }

    }
    //좌우 반전
    private void Turn()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        IsFacingRight = !IsFacingRight;
    }
    //이동
    private void Run(float lerpAmount)
    {
        //이동속도
        float runSpeed = moveInput.x * Data.runMaxSpeed;
     //   Debug.Log("moveInput.x : " + moveInput.x);
     //   Debug.Log("Data.runMaxSpeed : " + Data.runMaxSpeed);
        //이독속도 스케일
        runSpeed = Mathf.Lerp(rb.velocity.x, runSpeed, lerpAmount);
        //가속도 딜레이
        float accelRate;

        //가속도 결정
        if (LastOnGroundTime > 0)
            //이동속도가 0.01f 보다 빠를때 가속하고 아니면 감속
            accelRate = (Mathf.Abs(runSpeed) > 0.01f) ? Data.runAccelAmount : Data.runDeccelAmount;
        else
            //이동속도가 0.01f 보다 빠를때 가속하고 아니면 감속 
            accelRate = (Mathf.Abs(runSpeed) > 0.01f) ? Data.runAccelAmount * Data.accelInAir : Data.runDeccelAmount * Data.deccelInAir;
        
        //점프 중 가속도 조정
        if((IsJumping || IsWallJumping || isFalling) && Mathf.Abs(rb.velocity.y) < Data.jumpHangTimeThreshold)
        {
            accelRate *= Data.jumpHangAccelerationMult;
            runSpeed *= Data.jumpHangMaxSpeedMult;
        }
        //관성 유지 처리
        if(Data.doConserveMomantum && Mathf.Abs(rb.velocity.x) > Mathf.Abs(runSpeed) && Mathf.Sign(rb.velocity.x) == Mathf.Sign(runSpeed) && Mathf.Abs(runSpeed) > 0.01f && LastOnGroundTime < 0)
        {
            accelRate = 0;
        }

        float speedDif = runSpeed - rb.velocity.x;
        float movement = speedDif * accelRate;
     //   Debug.Log("movement : " + movement);

        rb.AddForce(movement * Vector2.right, ForceMode2D.Force);

    }
    //점프 인풋
    private void OnJumpInput()
    {
        //키 입력 버퍼 
        LastPressedJumpTime = Data.jumpInputBufferTime;
    }
    //이중 점프 인풋
    private void OnJumpUpInput()
    {
        if (CanJumpCut() || CanWallJumpCut())
            isJumpCut = true;
        Debug.Log("점프 컷 입력받음");
    }
    //대쉬 인풋
    private void OnDashInput()
    {
        //키 입력 버퍼
        LastPressedDashTime = Data.dashIputBufferTime;
    }
    //공격 입력
    private void OnAttackInput()
    {
        // 키 입력 버퍼
        isAttacked = true;
        LastPressedAttackTime = Data.attackInputBufferTime;
        Debug.Log("공격 입력");
    }
    public void OnComboNext()
    {
        comboTimer = comboResetTime;
        isAttacked = false;

        if (!IsJumping && !isFalling)
        {
            if (currentAttackIndex <= Data.AttackCombo)
                currentAttackIndex++;
            else
                currentAttackIndex = 0;
        }
        Debug.Log("OnComboNext 진입");
    }
    public void OnComboEnd()
    {
        currentAttackIndex = 0;
    }   
    //점프
    private void Jump()
    {
        LastPressedJumpTime = 0;
        LastOnGroundTime = 0;

        curJumpCount--;

        float jumpForce = Data.jumpForce;

        if(rb.velocity.y < 0)
            jumpForce -= rb.velocity.y;
        
        IsJumping = true;
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        Debug.Log("남은 점프 횟수" + curJumpCount);
    }
    //벽 점프
    private void WallJump(int dir)
    {
        LastPressedJumpTime = 0;
        LastOnGroundTime = 0;
        LastOnWallRightTime = 0;
        LastOnWallLeftTime = 0;


        Vector2 vel = rb.velocity;
        if (Mathf.Sign(vel.x) != dir) vel.x = 0;
        if (vel.y < 0) vel.y = 0;
        rb.velocity = vel;

        Vector2 force = new Vector2(Data.wallJumpForce.x, Data.wallJumpForce.y);
        force.x *= dir;

        IsWallJumping = true;
        rb.AddForce(force, ForceMode2D.Impulse);
        curJumpCount--;
        Debug.Log("남은 벽점프 횟수" + curJumpCount);
    }
    public void Attack()
    {
        if (CanAttack())
        {
            isAttacked = true;

            if (comboTimer <= 0)
            {
                OnComboEnd();
                Debug.Log("리셋 콤보 : " + currentAttackIndex);
            }
            Invoke("OnComboNext", 1f);
            Debug.Log("현재 콤보 : " + currentAttackIndex);
        }
    }
    private void DashAttack()
    {
        isDashAttacking = true;
        Invoke("DashAttackEnd", 0.5f);
    }
    private void DashAttackEnd()
    {
        isDashAttacking = false;
    }
    //죽음
    public void Died()
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
        Invoke("OffDamaged", 0.5f); 
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

        //대쉬 속도 감소
        rb.velocity = Data.dashEndSpeed * dir.normalized;

        //대쉬가 끝날때 까지의 시간
        while (Time.time - startTime <= Data.dashEndTime)
        {
            yield return null;
        }

        //중력스케일 복구
        SetGravityScale(Data.gravityScale);
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
        if(rb.velocity.y > 0)
        {
            rb.AddForce(rb.velocity.y * Vector2.up, ForceMode2D.Impulse);
        }

        float speedDif = Data.slideSpeed - rb.velocity.y;
        float movement = speedDif * Data.slideAccel;

        movement = Mathf.Clamp(movement, -Math.Abs(speedDif) * (1 / Time.fixedDeltaTime), Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime));

        rb.AddForce(movement * Vector2.up);
    }
    //중력 스케일 세팅
    private void SetGravityScale(float gravityScale)
    {
        rb.gravityScale = gravityScale;
    }
    //경직
    public void Freeze(float duration)
    {
        StartCoroutine(nameof(PerformFreeze), duration);
    }
    //경직 코루틴
    private IEnumerator PerformFreeze(float duration)
    {
        Time.timeScale = 0;
        yield return new WaitForSeconds(duration);
        Time.timeScale = 1;
    }
    //애니메이션 컨티션 값 입력
    public void UpdateConditions()
    {
        var animatorState = anim.GetCurrentAnimatorStateInfo(0);
        var velocity = rb.velocity;
        var sensitivity = 0.1f;

        //죽음 감지
        fsm.conditions.isDied = IsDied();
        //피격 감지
        fsm.conditions.isDamaged = isDamaged;
        //공격 감지
        fsm.conditions.isAttacked = isAttacked;
        //공격 콤보
        fsm.conditions.currentAttackIndex = currentAttackIndex;
        //바닥 감지
        fsm.conditions.isGrounded = isGrounded;
        //벽 슬라이드 감지
        fsm.conditions.isWalled = IsSliding;
        //x축 이동 감지
        fsm.conditions.moveDirection.x = (velocity.x > sensitivity ? 1 : 0) + (velocity.x < -sensitivity ? -1 : 0);
        //y축 이동 감지
        fsm.conditions.moveDirection.y = (velocity.y > sensitivity ? 1 : 0) + (velocity.y < -sensitivity ? -1 : 0);
        //대쉬 감지
        fsm.conditions.isDashing = IsDashing;
        //대쉬 공격 감지
        fsm.conditions.isDashAttacking = isDashAttacking;
        //전투 감지
        fsm.conditions.isFighting = IsFighting();

        fsm.Update(anim);

        var fsmAnimationName = fsm.currentState.animationName;
        //디버그
        if (!animatorState.IsName(fsmAnimationName))
        {
            anim.Play(fsmAnimationName);
        }
    }
    //죽음
    public bool IsDied()
    {
        if (Data.currentHp <= 0)
            return true;
        else
            return false;           
    }
    private void CheckDirectionToFace(bool isMovingRight)
    {
        if (isMovingRight != IsFacingRight)
            Turn();
    }
    //점프 조건
    private bool CanJump()
    {
        return (LastOnGroundTime > 0 || curJumpCount > 0);
    }
    //벽 점프
    private bool CanWallJump()
    {
        return LastPressedJumpTime > 0 && LastOnWallTime > 0 && LastOnGroundTime <= 0 && curJumpCount > 0 && !IsSliding && (!IsJumping || 
            (LastOnWallRightTime > 0 && lastWallJumpDir == 1) || (LastOnWallLeftTime > 0 && lastWallJumpDir == 1));
    }
    //이중 점프 조건
    private bool CanJumpCut()
    {
        return IsJumping && rb.velocity.y > 0 && curJumpCount > 0;
    }
    //벽 이중 점프 조건
    private bool CanWallJumpCut()
    {
        return IsWallJumping && rb.velocity.y > 0 && curJumpCount > 0;
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
    //공격 조건
    private bool CanAttack()
    {
        //피격 시 공격 제한
        if (isDamaged)
            return false;
        else
            return true;
    }
    private bool IsFighting()
    {
        if(comboTimer > 0 && comboTimer < 20f)
            return true;

        else return false;
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
