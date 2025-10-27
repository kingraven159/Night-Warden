using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using UnityEngine;

public class MonsterMovement : MonoBehaviour
{
    [SerializeField] private LayerMask ground;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    private int nextMove;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        Invoke("Think", 5);
    }
    private void FixedUpdate()
    {
        rb.velocity = new Vector2(nextMove, rb.velocity.y);

        Flip();

        //몬스터 앞 체크
        Vector2 frontVec = new Vector2(rb.position.x + nextMove * 0.2f, rb.position.y);
        Debug.DrawRay(frontVec, Vector3.down, new Color(0, 1, 0));

        RaycastHit2D hit = Physics2D.Raycast(frontVec, Vector3.down, 3, ground); 

        if (hit.collider == null)
        {
            Turn();
        }
    }
    private void Think()
    {
        //-1이면 왼쪽, 0이면 정지, 1이면 오른쪽으로 이동
        nextMove = Random.Range(-1, 2);

        float nextThinkTime = Random.Range(2f, 5f);

        Invoke("Think", nextThinkTime);

        anim.SetInteger("WalkSpeed", nextMove);
    }
    private void Flip()
    {
        if(nextMove != 0)
        {
            if(nextMove < 0)
            {
                sr.flipX = true;
            }
            else
            {
                 sr.flipX= false;
            }
        }
    }
    private void Turn()
    {
        nextMove = nextMove * (-1);
        CancelInvoke();
        Invoke("Thike", 2);
    }
    
}
