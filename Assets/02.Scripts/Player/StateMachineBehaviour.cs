using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateMachineBehaviour : ScriptableObject
{
    public virtual void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
    { 
        //상태에 진입할 때 호출 
    }
    public virtual void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
    {
        //상태가 업데이트될 때 호출 
    }
    public virtual void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
    { 
        //상태가 빠져나갈때 호출 
    }
    public virtual void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    { 
        //상태가 이동할 때 호출 
    }
    public virtual void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
    { 
        //역방향 키네마틱(Inverse Kinematics) 작업을 수행할 때 호출 
    }
    public virtual void OnStateMachineEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
    { 
        //상태 머신에 진입할 때 호출 
    }
    public virtual void OnStateMachineExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
    { 
        //상태 머신을 빠져나갈 때 호출 
    }
}



