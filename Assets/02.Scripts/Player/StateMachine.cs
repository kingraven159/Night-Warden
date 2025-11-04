using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace AnimationFSM
{
    //컨디션 조건
    public struct Conditions
    {
        public bool isGrounded;
        public bool isAttacked;
        public bool isFighting;
        public bool isDashing;
        public bool isDamaged;
        public bool isWalled;
        public bool isDied;
        public bool isDashAttacking;

        public int currentAttackIndex;
        public Vector2Int moveDirection;

        public bool IsFalling => !isGrounded && moveDirection.y < 0;
        public bool IsJumping => !isGrounded && moveDirection.y > 0;
        public bool IsRunning => moveDirection.x != 0;
    }
    //각 상태의 베이스 클래스
    public abstract class State
    {
        public string animationName { get; }
        // 상태 우선순위 (높을수록 우선 적용)
        public int priority { get; }
        protected float blendTime = 0.1f;

        //콜백 이벤트
        public event Action OnEnterEvent;
        public event Action OnExitEvent;

        protected State(string animationName, int priority = 0)
        {
            this.animationName = animationName;
            this.priority = priority;
        }

        // 상태 시작 시 호출
        public virtual void OnEnter(Animator animator, State previousState = null)
        {
            //애니메이션 블렌딩 처리
            if (previousState != null && previousState.animationName != animationName)
                animator.CrossFadeInFixedTime(animationName, blendTime);
            else
                animator.Play(animationName);

            //이벤트 호출
            OnEnterEvent?.Invoke();
        }

        public virtual void OnExit() 
        {
            OnExitEvent?.Invoke();
        }
        public virtual void OnUpdate() { }

        // 전환 조건
        public abstract bool IsMatchingConditions(Conditions con);

        public State OnEnter(Action callback)
        {
            OnEnterEvent += callback;
            return this;
        }
        public State OnExit(Action callback)
        {
            OnExitEvent += callback;
            return this;
        }
    }
    //유한 상태 머신
    public class FSM
    {
        private List<State> states = new();
        private State currentState;
        private State previousState;
        public Conditions conditions;
        public bool forcedChange;

        //현재 상태 읽기 전용
        public State CurrentState => currentState;

        public void AddState(State state)
        {
            states.Add(state);
            // 우선순위 높은 순으로 정렬
            states.Sort((a, b) => b.priority.CompareTo(a.priority));
        }

        public void Update(Animator animator)
        {
            if(!forcedChange && currentState != null)
            {
                currentState.OnUpdate();
                return;
            }

            State nextState = null;

            for (int i = 0; i < states.Count; i++)
            {
                var s = states[i];
                if (s.IsMatchingConditions(conditions))
                {
                    nextState = s;
                    break;
                }
            }
            if (nextState == null)
                return;
            if(nextState != currentState)
            {
                previousState = currentState;
                currentState?.OnExit();
                currentState = nextState;
                currentState.OnEnter(animator, previousState);
                forcedChange = false;
            }
        }
        //상태 변경을 강제로 트리거 할때 사용
        public void ForceTransition()
        {
            forcedChange = true;
        }
        //상태 초기화
        public void Reset()
        {
            currentState = null;
            previousState = null;
            forcedChange = true;
        }
    }
}






