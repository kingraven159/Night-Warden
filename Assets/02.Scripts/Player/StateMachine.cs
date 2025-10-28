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
        public Vector2Int moveDirection;
        public bool isAttacked;
        public bool isFighting;
        public int numOfAttack;
        public bool isDashing;

    }
    public abstract class State
    {
        public string animationName;
        //애니메이션 이름
        protected State(string animationName)
        {
            this.animationName = animationName;
        }
        //상태머신의 조건이 맞는지 판별해주는 추상 메서드
        public abstract bool IsMatchingCondtions(Conditions conditions);
    }
    //유한 상태 머신
    public class FSM
    {
        //컨디션
        public Conditions conditions;
        //현재 컨디션 상태
        public State currentState;
        List<State> states = new List<State>();

        public void AddState(State state)
        {
            states.Add(state);
        }

        public void Update()
        {
            foreach (var state in states)
            {
                if (state.IsMatchingCondtions(conditions))
                {
                    currentState = state;
                    break;
                }
            }
        }
    }
}






