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

    //�̵� ����
    private float inputX;
    private bool isGrounded;
    private bool jumpPressed;

    //�뽬 ����
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
        //�ٴ� ����
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        Move();
        Jump();
        Flip();
        OnDash();
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
        if(isAbleDash && unLockedDash)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
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
        rb.gravityScale = 0;
        //�ܻ� �����
        this.ghost.makeGhost = true;
        rb.velocity = new Vector2(transform.localScale.x * dashPower, 0);
        isDashing = false;
        //�ܻ� ��
        this.ghost.makeGhost = false;
        //�߷� �ʱ�ȭ
        rb.gravityScale = oringnalGravity;
        //�ൿ �Ұ�
        rb.velocity = new Vector2(0, 0);
        yield return new WaitForSeconds(dashCoolDown);
        isAbleDash = true;

        Debug.Log("Dash Cool Time Finished");
    }
    //����
    public void Die()
    {

    }
}
