using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MontserBehaviorTree : MonoBehaviour
{
    INode _rootNode;
    public MontserBehaviorTree(INode rootNode)
    {
        _rootNode = rootNode;
    }

    public void Operate()
    {
        _rootNode.Evaluate();
    }
}
[RequireComponent(typeof(Animator))]
public class EnemyAI : MonoBehaviour
{
    [Header("Range")]
    [SerializeField] private float _detectRange = 10f;
    [SerializeField] private float _meleeAttackRange = 5f;

    [Header("Movement")]
    [SerializeField] private float _movementSpeed = 10f;

    Vector3 originPos = default;
    MontserBehaviorTree BTRunner = null;
    Transform detectedPlayer = null;
    Animator animator = null;

    const string _ATTACK_ANIM_STATE_NAME = "Attack";
    const string _ATTACK_ANIM_TIRGGER_NAME = "attack";

    private void Awake()
    {
        animator = GetComponent<Animator>();

        BTRunner = new MontserBehaviorTree(SettingBT());

        originPos = transform.position;
    }

    private void Update()
    {
        BTRunner.Operate();
    }

    INode SettingBT()
    {
        return new SelectorNode
            (
                new List<INode>()
                {
                    new SequenceNode
                    (
                        new List<INode>()
                        {
                            new ActionNode(CheckMeleeAttacking),
                            new ActionNode(CheckEnemyWithinMeleeAttackRange),
                           // new ActionNode(DoMeleeAttack),
                        }
                    ),
                    new SequenceNode
                    (
                        new List<INode>()
                        {
                            new ActionNode(CheckDetectEnemy),
                            new ActionNode(MoveToDetectEnemy),
                        }
                    ),
                    new ActionNode(MoveToOriginPosition)
                }
            );
    }

    bool IsAnimationRunning(string stateName)
    {
        if (animator != null)
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
            {
                var normalizedTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

                return normalizedTime != 0 && normalizedTime < 1f;
            }
        }

        return false;
    }


    INode.ENodeState CheckMeleeAttacking()
    {
        if (IsAnimationRunning(_ATTACK_ANIM_STATE_NAME))
        {
            return INode.ENodeState.Running;
        }

        return INode.ENodeState.Success;
    }

    INode.ENodeState CheckEnemyWithinMeleeAttackRange()
    {
        if (detectedPlayer != null)
        {
            if (Vector3.SqrMagnitude(detectedPlayer.position - transform.position) < (_meleeAttackRange * _meleeAttackRange))
            {
                return INode.ENodeState.Success;
            }
        }

        return INode.ENodeState.Failure;
    }

  //  INode.ENodeState DoMeleeAttack()
  //  {
  //      if (detectedPlayer != null)
  //      {
  //          animator.SetTrigger(_ATTACK_ANIM_TIRGGER_NAME);
  //          return INode.ENodeState.Success;
  //      }
  //
  //      return INode.ENodeState.Failure;
  //  }

    INode.ENodeState CheckDetectEnemy()
    {
        var overlapColliders = Physics.OverlapSphere(transform.position, _detectRange, LayerMask.GetMask("Player"));

        if (overlapColliders != null && overlapColliders.Length > 0)
        {
            detectedPlayer = overlapColliders[0].transform;

            return INode.ENodeState.Success;
        }

        detectedPlayer = null;

        return INode.ENodeState.Failure;
    }

    INode.ENodeState MoveToDetectEnemy()
    {
        if (detectedPlayer != null)
        {
            if (Vector3.SqrMagnitude(detectedPlayer.position - transform.position) < (_meleeAttackRange * _meleeAttackRange))
            {
                return INode.ENodeState.Success;
            }

            transform.position = Vector3.MoveTowards(transform.position, detectedPlayer.position, Time.deltaTime * _movementSpeed);

            return INode.ENodeState.Running;
        }

        return INode.ENodeState.Failure;
    }

    INode.ENodeState MoveToOriginPosition()
    {
        if (Vector3.SqrMagnitude(originPos - transform.position) < float.Epsilon * float.Epsilon)
        {
            return INode.ENodeState.Success;
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, originPos, Time.deltaTime * _movementSpeed);
            return INode.ENodeState.Running;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(this.transform.position, _detectRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(this.transform.position, _meleeAttackRange);
    }
}