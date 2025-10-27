using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class SlimeMovement : MonoBehaviour
{
    [Header("monster state")]
    protected int currentHp;
    protected float moveSpeed;
    protected int damage;


    [Header("raycast setup")]
    [SerializeField] private float raycastDis = 1f;

    private Rigidbody2D rb;

    public void Initalize(MonsterData monsterData)
    {
        currentHp = monsterData.Hp;
        moveSpeed = monsterData.MoveSpeed;
        damage = monsterData.Damage;
    }
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    private void FixedUpdate()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, -transform.up, raycastDis);

        if (hit.collider != null)
        {
            Vector2 surfaceNormal = hit.normal;
            Vector2 surfaceTangent = Vector2.Perpendicular(surfaceNormal);

            Quaternion newRotation = Quaternion.FromToRotation(transform.up, surfaceNormal) * transform.rotation;
            transform.rotation = newRotation;

            rb.velocity = surfaceTangent * moveSpeed;
        }
        else 
        {
            rb.velocity = new Vector2(rb.velocity.x, -10f);
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, -transform.up * raycastDis);
    }
}

