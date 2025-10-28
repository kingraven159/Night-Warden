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

    //�̵� ����
    private float inputX;
    private float inputY;
    private bool isGrounded;
    private bool jumpPressed;
    private bool isWalled;

    //�뽬 ����
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
        //�ٴ� ����
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        Move();
        Flip();
        Attack();
    }
    //�¿� ����
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
    //�̵�
    private void Move()
    {
        rb.velocity = new Vector2(inputX * moveSpeed , rb.velocity.y);

    }
    //����
    private void Jump()
    {
        if(jumpPressed && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
        jumpPressed = false;
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
    //�ǰݽ� ��������
    private void OnDamaged(Vector2 targetPos)
    {
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
    }
    //�뽬 �Է�
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
    //�뽬 �ڷ�ƾ
    public IEnumerator Dash()
    {
        Debug.Log("Dash");
        Debug.Log("Dash Cool Time Start");

        isAbleDash = false;
        isDashing = true;
        //���� �߷°� ���
        var oringnalGravity = rb.gravityScale;
        //�߷� = 0;
        rb.gravityScale = 0f;
        //�ܻ� �����
        rb.velocity = new Vector2(transform.localScale.x * dashPower, 0f);
        yield return new WaitForSeconds(dashingTime);
        isDashing = false;
        //�ܻ� ��
        //�߷� �ʱ�ȭ
        rb.gravityScale = oringnalGravity;
        //�ൿ �Ұ�
        rb.velocity = new Vector2(0, 0);
        Debug.Log("�߷� ��" + rb.gravityScale);
        yield return new WaitForSeconds(dashCoolDown);
        isAbleDash = true;

        Debug.Log("Dash Cool Time Finished");
    }
    //����
    private void Attack()
    {
        if(Input.GetKeyDown(KeyCode.K) && !isAttacked)
        {
            isAttacked = true;
            
        }
    }
    //�ִϸ��̼�
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
        fsm.conditions.isDashing = isDashing;
        //���� ����
        fsm.conditions.isAttacked = isAttacked;

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
}
