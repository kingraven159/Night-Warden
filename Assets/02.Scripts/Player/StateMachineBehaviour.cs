using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateMachineBehaviour : ScriptableObject
{
    public virtual void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
    { 
        //���¿� ������ �� ȣ�� 
    }
    public virtual void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
    {
        //���°� ������Ʈ�� �� ȣ�� 
    }
    public virtual void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
    { 
        //���°� ���������� ȣ�� 
    }
    public virtual void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    { 
        //���°� �̵��� �� ȣ�� 
    }
    public virtual void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
    { 
        //������ Ű�׸�ƽ(Inverse Kinematics) �۾��� ������ �� ȣ�� 
    }
    public virtual void OnStateMachineEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
    { 
        //���� �ӽſ� ������ �� ȣ�� 
    }
    public virtual void OnStateMachineExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
    { 
        //���� �ӽ��� �������� �� ȣ�� 
    }
}



