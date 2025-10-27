using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public MonsterData monsterData;
    
    public void PrintMonsterData()
    {
        Debug.Log("���� �̸� : " + monsterData.MonsterName);
        Debug.Log("ü�� : " + monsterData.Hp);
        Debug.Log("���ݷ� : " + monsterData.Damage);
        Debug.Log("�þ� : " + monsterData.SightRange);
        Debug.Log("�̵��ӵ� : " + monsterData.MoveSpeed);
    }
}
