using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    public int value = 1;
    public float bouncForce = 2f;

    private void Start()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if(rb != null)
        {
            rb.AddForce(new Vector2(Random.Range(-1f, 1f), 1f) * bouncForce, ForceMode2D.Impulse);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            //플레이어의 코인 획득 함수 호출
            PlayerInventory inventory = collision.GetComponent<PlayerInventory>();
            if(inventory != null)
            {
                inventory.AddCoins(value);
            }
            Destroy(gameObject);
        }
    }
}
