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

    //����
    public bool IsAttachedWall {  get; private set; }
    public bool IsFacingRight {  get; private set; }
    public bool IsJumping { get; private set; }
    public bool IsWallJumping { get; private set; }
    public bool IsDashing { get; private set; }
    public bool IsSliding { get; private set; }

    //Ÿ�̸�
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

    //�������� �Է��� �ð�
    public float LastPressedJumpTime { get; private set; }
    public float LastPressedDashTime { get; private set; }  

    //�̵� ����
    private Vector2 moveInput;
    private bool isGrounded;

    //���� ����
    private bool isFalling;
    private bool isJumpCut;

    //������ ����
    private float wallJumpStartTime;
    private int lastWallJumpDir;

    //�뽬 ����
    public bool unLockedDash;
    private int dashesCount;
    private bool dashRefilling;
    private Vector2 lastDashDir;
    private bool isDashAttacking;

    //���� ����
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

        //�ִϸ��̼� ������Ʈ
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
        //�߷� �� ����
        SetGravityScale(Data.gravityScale);
        IsFacingRight = true;
    }
    private void Update()
    {
        //Ÿ�̸�
        LastOnGroundTime -= Time.deltaTime;
        LastOnWallTime -= Time.deltaTime;
        LastOnWallRightTime -= Time.deltaTime;
        LastOnWallLeftTime -= Time.deltaTime;

        LastPressedJumpTime -= Time.deltaTime;
        LastPressedDashTime -= Time.deltaTime;


        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        if (moveInput.x != 0)
            CheckDirectionToFace(moveInput.x > 0);

        //����
        if(Input.GetButtonDown("Jump"))
        {
            OnJumpInput();
        }
        //���� ����
        if(Input.GetButtonUp("Jump"))
        {
            OnJumpUpInput();
        }
        //�뽬
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
                    isFalling = false;
                }
                LastOnGroundTime = Data.coyoteTime;
            }
            if((IsAttachedWall && IsFacingRight)
                || (IsAttachedWall && IsFacingRight) && IsWallJumping)
                LastOnWallRightTime = Data.coyoteTime;
            if((IsAttachedWall && IsFacingRight)
                || (IsAttachedWall && IsFacingRight) && IsWallJumping)
                LastOnWallLeftTime = Data.coyoteTime;

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

        if(!IsDashing)
        {
            if(CanJump() && LastOnGroundTime > 0)
            {
                IsJumping = true;
                IsWallJumping = false;
                isJumpCut = false;
                isFalling = false;
                Jump();
            }
            else if(CanWallJump() && LastOnGroundTime > 0)
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

        if (CanDash() && LastOnGroundTime > 0)
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
            IsSliding = true;
        else
            IsSliding = false;

        if (!isDashAttacking)
        {
            //�������� �������� 
            if (IsSliding)
            {
                SetGravityScale(0);
            }
            else if (rb.velocity.y < 0 && moveInput.y < 0)
            {
                //�߷� �������� �� ũ�� ����
                SetGravityScale(Data.gravityScale * Data.fastFallGravityMult);
                //�������� �ְ� �ӵ� 
                rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -Data.maxFastFallSpeed));
            }
            else if (isJumpCut)
            {
                SetGravityScale(Data.gravityScale * Data.jumpCutGravityMult);
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

        AnimationCondtion();
    }
    private void FixedUpdate()
    {
        //�ٴ� ����
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        //�� ����
        IsAttachedWall = Physics2D.OverlapBox(frontWallCheckPoint.position, wallCheckSize, 0, groundLayer) || Physics2D.OverlapBox(backWallCheckPoint.position, wallCheckSize, 0, groundLayer);

        //������ ���� �ʰ� �ٴڿ� �پ������� �ڿ��� Ÿ��
        if(!IsJumping && isGrounded)  
            LastOnGroundTime = Data.coyoteTime;        
        if (IsAttachedWall)
            LastOnGroundTime = Data.coyoteTime;

        if (!IsDashing)
        {
            //�� ���� ���϶�
            if (IsWallJumping)
                Run(Data.wallJumpRunLerp);
            else
                Run(1);
        }
        else if (isDashAttacking)
        {
            Run(Data.dashEndRunLerp);
        }
            //�� �����̵� 
        if (IsSliding)
            Slide();
        
        

        Attack();
    }
    //�¿� ����
    private void Flip()
    {
        if(moveInput.x != 0)
        {
            if(moveInput.x < 0)
            {
                spriteRenderer.flipX = true;
            }
            else
            {
                spriteRenderer.flipX=false;
            }
        }
    }
    //�̵�
    private void Run(float lerpAmount)
    {
        //�̵��ӵ�
        float runSpeed = moveSpeed * Data.runMaxSpeed;
        //�̵��ӵ� ������
        runSpeed = Mathf.Lerp(rb.velocity.x, runSpeed, lerpAmount);
        //���ӵ� ������
        float accelRate;

        //���� ������
        if (LastOnGroundTime > 0)
            //�̵��ӵ��� 0.01f ���� ������ �����ϰ� �ƴϸ� ����
            accelRate = (Mathf.Abs(runSpeed) > 0.01f) ? Data.runAccelAmount : Data.runDeccelAmount;
        else
            //�̵��ӵ��� 0.01f ���� ������ �����ϰ� �ƴϸ� ���� 
            accelRate = (Mathf.Abs(runSpeed) > 0.01f) ? Data.runAccelAmount * Data.accelInAir : Data.runDeccelAmount * Data.deccelInAir;
        
        //������ �߶����϶� 
        if((IsJumping || IsWallJumping || isFalling) && Mathf.Abs(rb.velocity.y) < Data.jumpHangTimeThreshold)
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
    //���� ��ǲ
    private void OnJumpInput()
    {
        //���۸� 
        LastPressedJumpTime = Data.jumpInputBufferTime;
    }
    //���� ���� ��ǲ
    private void OnJumpUpInput()
    {
        if (CanJumpCut() || CanWallJumpCut())
            isJumpCut = true;
    }
    //�뽬 ��ǲ
    private void OnDashInput()
    {
        //���۸�
        LastPressedDashTime = Data.dashIputBufferTime;
    }
    //����
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
    //�� ����
    private void WallJump(int dir)
    {
        
    }
    //���� �浹 ����
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Enemy")
        {
            Debug.Log(collision.gameObject.tag + "�� �ε���");
            OnDamaged(collision.transform.position);
        }
    }
    //�ǰݽ� ������ & ��������
    private void OnDamaged(Vector2 targetPos)
    {
        isDamaged = true;
        //�÷��̾��� Layer�� ����
        gameObject.layer = 8;

        //�ǰݽ� �����ϰ� ����
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);
        //�ǰݽ� ƨ�ܳ����� ���� ����
        int dirc = transform.position.x - targetPos.x > 0 ? 1 : -1;
        //�¿�� ƨ��
        rb.AddForce(new Vector2(dirc, 1) * 3, ForceMode2D.Impulse);

        //1�� �� ���� �ð� ��
        Invoke("OffDamaged", 1); 
    }
    //�����ð� ��
    private void OffDamaged()
    {
        //�÷��̾� Layer ���� ����
        gameObject.layer = 7;
        //���� ���� ����
        spriteRenderer.color = new Color(1, 1, 1, 1f);
        isDamaged = false;
    }
    //�뽬 �ڷ�ƾ
    public IEnumerator StartDash(Vector2 dir)
    {
        LastOnGroundTime = 0;
        LastPressedDashTime = 0;

        Debug.Log("Dash");
        Debug.Log("Dash Cool Time Start");

        float startTime = Time.time;

        dashesCount--;
        //�뽬 ���� Ȱ��ȭ
        isDashAttacking = true;

        //���߷�
        SetGravityScale(0);

        //�뽬������ ������ �ð�
        while (Time.time - startTime <= Data.dashAttackTime)
        {
            rb.velocity = dir.normalized * Data.dashSpeed;
            yield return null;
        }

        startTime = Time.time;
        isDashAttacking = false;

        //�߷½����� ����
        SetGravityScale(Data.gravityScale);
        //�뽬 �ӵ� ����
        rb.velocity = Data.dashEndSpeed * dir.normalized;

        //�뽬�� ������ ������ �ð�
        while (Time.time - startTime <= Data.dashEndTime)
        {
            yield return null;
        }

        //�뽬 ��
        IsDashing = false;

        Debug.Log("Dash Cool Time Finished");
    }
    //�뽬 ����
    private IEnumerator RefillDash(int amount)
    {
        //�뽬 ���� ��
        dashRefilling = true;
        yield return new WaitForSeconds(Data.dashRefillTime);
        //�뽬 ���� ��
        dashRefilling = false;
        dashesCount = Mathf.Min(Data.dashAmount, dashesCount + 1);
    }
    //�� �����̵�
    private void Slide()
    {
        // y�� �ӵ��� 0���� ũ�� 
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
    //����
    private void Attack()
    {
        if(Input.GetKeyDown(KeyCode.K) && !isAttacked)
        {
            isAttacked = true;
            
        }
    }
    //�߷� ������ ����
    private void SetGravityScale(float gravityScale)
    {
        rb.gravityScale = gravityScale;
    }
    //����
    public void Freeze(float duration)
    {
        StartCoroutine(nameof(PerformFreeze), duration);
    }
    //���� �ڷ�ƾ
    private IEnumerator PerformFreeze(float duration)
    {
        Time.timeScale = 0;
        yield return new WaitForSeconds(duration);
        Time.timeScale = 1;
    }
    //�ִϸ��̼� ��Ƽ�� �� �Է�
    private void AnimationCondtion()
    {
        var animatorState = anim.GetCurrentAnimatorStateInfo(0);
        var velocity = rb.velocity;
        var sensitivity = 0.1f;

        //�ٴ� ����
        fsm.conditions.isGrounded = isGrounded;
        //x�� �̵� ����
        fsm.conditions.moveDirection.x = (velocity.x > sensitivity ? 1 : 0) + (velocity.x < -sensitivity ? -1 : 0);
        //y�� �̵� ����
        fsm.conditions.moveDirection.y = (velocity.y > sensitivity ? 1 : 0) + (velocity.y < -sensitivity ? -1 : 0);
        //�뽬 ����
        fsm.conditions.isDashing = IsDashing;
        //���� ����
        fsm.conditions.isAttacked = isAttacked;
        //�ǰ� ����
        fsm.conditions.isDamaged = isDamaged;

        fsm.Update();

        var fsmAnimationName = fsm.currentState.animationName;
        //�����
        if (!animatorState.IsName(fsmAnimationName))
        {
            anim.Play(fsmAnimationName);
        }
    }
    //����
    public void Die()
    {

    }
    private void CheckDirectionToFace(bool isMovingRight)
    {
        if (isMovingRight != IsFacingRight)
            Flip();
    }
    //���� ����
    private bool CanJump()
    {
        return LastPressedJumpTime > 0 && !IsJumping;
    }
    //�� ����
    private bool CanWallJump()
    {
        return LastPressedJumpTime > 0 && LastOnWallTime > 0 && LastOnGroundTime <= 0 && (!IsWallJumping || 
            (LastOnWallRightTime > 0 && lastWallJumpDir == 1) || (LastOnWallLeftTime > 0 && lastWallJumpDir == -1));
    }
    //���� ���� ����
    private bool CanJumpCut()
    {
        return IsJumping && rb.velocity.y > 0;
    }
    //�� ���� ���� ����
    private bool CanWallJumpCut()
    {
        return IsWallJumping && rb.velocity.y > 0;
    }
    //�뽬 ����
    private bool CanDash()
    {
        if(!IsDashing && dashesCount < Data.dashAmount && LastOnGroundTime > 0 && !dashRefilling)
        {
            StartCoroutine(nameof(RefillDash), 1);
        }
        return dashesCount > 0;
    }
    //�� �����̵� ����
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
