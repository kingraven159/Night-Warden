using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;



namespace AnimationFSM
{
    //����� ����
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
        //�ִϸ��̼� �̸�
        protected State(string animationName)
        {
            this.animationName = animationName;
        }
        //���¸ӽ��� ������ �´��� �Ǻ����ִ� �߻� �޼���
        public abstract bool IsMatchingCondtions(Conditions conditions);
    }
    //���� ���� �ӽ�
    public class FSM
    {
        //�����
        public Conditions conditions;
        //���� ����� ����
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






