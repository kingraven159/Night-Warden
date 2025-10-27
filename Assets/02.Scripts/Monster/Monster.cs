using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public MonsterData monsterData;
    
    public void PrintMonsterData()
    {
        Debug.Log("몬스터 이름 : " + monsterData.MonsterName);
        Debug.Log("체력 : " + monsterData.Hp);
        Debug.Log("공격력 : " + monsterData.Damage);
        Debug.Log("시야 : " + monsterData.SightRange);
        Debug.Log("이동속도 : " + monsterData.MoveSpeed);
    }
}
