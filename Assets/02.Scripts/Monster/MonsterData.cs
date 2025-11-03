using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "Monster Data", menuName = "Scriptable Object/Monster Data", order = int.MaxValue)]
public class MonsterData : ScriptableObject
{
    [Header("몬스터 정보")]
    [SerializeField] private string monsterName;
    public string MonsterName { get { return monsterName; } }
    [SerializeField] private int maxHp;
    public int MaxHp { get { return maxHp; } }
    [SerializeField] private int damage;
    public int Damage { get { return damage; } }
    [SerializeField] private float moveSpeed;
    public float MoveSpeed { get { return moveSpeed; } }

    [Space(20)]
    [Header("AI 범위")]
    [SerializeField] private float detectionRange;
    public float DetectionRange { get { return detectionRange; } }
    [SerializeField] private float attackRange;
    public float AttackRange { get { return attackRange; } }
    [SerializeField] private float jumpAttackRange;
    public float JumpAttackRange { get {return jumpAttackRange; } }

    [Header("패트롤 설정")]
    [SerializeField] private float patrolRadius;
    public float PatrolRadius { get { return patrolRadius; } }
    [SerializeField] private float patrolWaitTime;
    public float PatrolWaitTime { get { return patrolWaitTime; } }

    public GameObject prefab;
}
