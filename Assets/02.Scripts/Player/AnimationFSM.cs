using System;
using UnityEngine.Rendering;

namespace AnimationFSM
{
    public class Attack1State : State
    {
        public Attack1State(string animationName) : base(animationName) { }
        public override bool IsMatchingCondtions(Conditions conditions)
        {
            return conditions.isAttacked;
        }
    }
    public class Attack2State : State
    {
        public Attack2State(string animationName) : base(animationName) { }
        public override bool IsMatchingCondtions(Conditions conditions)
        {
            return conditions.isAttacked;
        }
    }
    public class Attack3State : State
    {
        public Attack3State(string animationName) : base(animationName) { }
        public override bool IsMatchingCondtions(Conditions conditions)
        {
            return conditions.isAttacked;
        }
    }
    public class AttackUpState : State
    {
        public AttackUpState(string animationName) : base(animationName) { }
        public override bool IsMatchingCondtions(Conditions conditions)
        {
            throw new NotImplementedException();
        }   
    }
    public class AttackDownState : State
    {
        public AttackDownState(string animationName) : base(animationName) { }
        public override bool IsMatchingCondtions(Conditions conditions)
        {
            throw new NotImplementedException();
        }
    }
    public class FallState : State
    {
        public FallState(string animationName) : base(animationName) { }
        public override bool IsMatchingCondtions(Conditions conditions)
        {
            return !conditions.isGrounded && conditions.moveDirection.y == -1;
        }
    }
    public class JumpState : State
    {
        public JumpState(string animationName) : base(animationName) { }
        public override bool IsMatchingCondtions(Conditions conditions)
        {
            return !conditions.isGrounded && conditions.moveDirection.y == 1;
        }

    }
    public class RunState : State 
    {
        public RunState(string animationName) : base(animationName) { }
        public override bool IsMatchingCondtions(Conditions conditions)
        {
            return conditions.moveDirection.x != 0  && !conditions.isDashing;
        }
    }
    public class DashState : State
    {
        public DashState(string animationName) : base(animationName) { }
        public override bool IsMatchingCondtions(Conditions conditions)
        {
            return !conditions.isGrounded && conditions.isDashing;
        }

    }
    public class SlideState : State
    {
        public SlideState(string animationName) : base(animationName) { }
        public override bool IsMatchingCondtions(Conditions conditions)
        {
            return conditions.isGrounded && conditions.isDashing;
        }

    }
    public class IdleState : State
    {
        public IdleState(string animationName) : base(animationName) { }
        public override bool IsMatchingCondtions(Conditions conditions)
        {
            return true;
        }
    }
}
