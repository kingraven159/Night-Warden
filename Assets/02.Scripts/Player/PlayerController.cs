using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float jumpForce = 7.0f;

    [Header("GroundCheck")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Dash")]
    [SerializeField] private float dashPower = 5.0f;
    [SerializeField] private float dashCoolDown = 1f;

    //이동 관련
    private float inputX;
    private bool isGrounded;
    private bool jumpPressed;

    //대쉬 관련
    public bool unLockedDash;
    private bool isAbleDash;
    private bool isDashing;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Ghost ghost;

    private bool isDamaged;

    private static readonly int speedHash = Animator.StringToHash("Speed");
    private static readonly int fallingHash = Animator.StringToHash("Falling");
    private static readonly int isDashingHash = Animator.StringToHash("isDashing");
    private static readonly int jumpHash = Animator.StringToHash("Jumping");
    private void Awake()
    {

        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    private void Update()
    {
        inputX = Input.GetAxisRaw("Horizontal");

        if(Input.GetButton("Jump"))
        {
            jumpPressed = true;
        }
    }
    private void FixedUpdate()
    {
        //바닥 판정
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        Move();
        Jump();
        Flip();
        OnDash();
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
        if(isAbleDash && unLockedDash)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
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
        rb.gravityScale = 0;
        //잔상 만들기
        this.ghost.makeGhost = true;
        rb.velocity = new Vector2(transform.localScale.x * dashPower, 0);
        isDashing = false;
        //잔상 끝
        this.ghost.makeGhost = false;
        //중력 초기화
        rb.gravityScale = oringnalGravity;
        //행동 불가
        rb.velocity = new Vector2(0, 0);
        yield return new WaitForSeconds(dashCoolDown);
        isAbleDash = true;

        Debug.Log("Dash Cool Time Finished");
    }
    //죽음
    public void Die()
    {

    }
}
