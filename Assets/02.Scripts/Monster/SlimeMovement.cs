using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class SlimeMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool isStuck;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(!isStuck && collision.gameObject.CompareTag("Ground"))
        {

        }
    }
}

