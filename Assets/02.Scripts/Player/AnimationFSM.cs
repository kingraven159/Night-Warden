using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace AnimationFSM
{
    public class DieState : State
    {
        public DieState(string animatorName) : base(animatorName, 10) { }
        public override bool IsMatchingConditions(Conditions conditions)
        {
            return conditions.isDied;
        }
    }
    public class HurtState : State
    {
        public HurtState(string animationName) : base(animationName, 9) { }
        public override bool IsMatchingConditions(Conditions conditions)
        {
            return conditions.isDamaged;
        }
    }
    public class AttackRunState : State
    {
        public AttackRunState(string animationName) : base(animationName, 8) { }
        public override bool IsMatchingConditions(Conditions conditions)
        {
            return conditions.isAttacked && conditions.isDashAttacking;
        }
    }
    public class AttackUpState : State
    {
        public AttackUpState(string animationName) : base(animationName, 7) { }
        public override bool IsMatchingConditions(Conditions conditions)
        {
            return conditions.isAttacked && conditions.moveDirection.y == 1;
        }
    }
    public class AttackDownState : State
    {
        public AttackDownState(string animationName) : base(animationName, 7) { }
        public override bool IsMatchingConditions(Conditions conditions)
        {
            return conditions.isAttacked && conditions.moveDirection.y == -1;
        }
    }
    public class Attack3State : State
    {
        public Attack3State(string animationName) : base(animationName, 6) { }
        public override bool IsMatchingConditions(Conditions conditions)
        {
            return conditions.isAttacked && conditions.currentAttackIndex == 2 && !conditions.isDashAttacking;
        }
    }
    public class Attack2State : State
    {
        public Attack2State(string animationName) : base(animationName, 5) { }
        public override bool IsMatchingConditions(Conditions conditions)
        {
            return conditions.isAttacked && conditions.currentAttackIndex == 1 && !conditions.isDashAttacking;
        }
    }
    public class Attack1State : State
    {
        public Attack1State(string animationName) : base(animationName, 4) { }
        public override bool IsMatchingConditions(Conditions conditions)
        {
            return conditions.isAttacked && conditions.currentAttackIndex == 0 && !conditions.isDashAttacking;
        }
    }
    public class WallSlideState : State
    {
        public WallSlideState(string animationName) : base(animationName, 3) { }
        public override bool IsMatchingConditions(Conditions conditions)
        {
            return conditions.isWalled && !conditions.isGrounded && !conditions.IsJumping && conditions.IsFalling;
        }
    }
    public class FallState : State
    {
        public FallState(string animationName) : base(animationName, 2) { }
        public override bool IsMatchingConditions(Conditions conditions)
        {
            return !conditions.isGrounded && conditions.IsFalling && !conditions.isWalled;
        }
    }
    public class JumpState : State
    {
        public JumpState(string animationName) : base(animationName, 2) { }
        public override bool IsMatchingConditions(Conditions conditions)
        {
            return !conditions.isGrounded && conditions.IsJumping;
        }

    }
    public class DashState : State
    {
        public DashState(string animationName) : base(animationName, 2) { }
        public override bool IsMatchingConditions(Conditions conditions)
        {
            return !conditions.isGrounded && conditions.isDashing;
        }

    }
    public class SlideState : State
    {
        public SlideState(string animationName) : base(animationName, 2) { }
        public override bool IsMatchingConditions(Conditions conditions)
        {
            return conditions.isGrounded && conditions.isDashing;
        }

    }
    public class RunState : State
    {
        public RunState(string animationName) : base(animationName, 1) { }
        public override bool IsMatchingConditions(Conditions conditions)
        {
            return conditions.IsRunning && !conditions.isDashing && !conditions.isFighting;
        }
    }
    public class Run2State : State
    {
        public Run2State(string animationName) : base(animationName, 1) { }
        public override bool IsMatchingConditions(Conditions conditions)
        {
            return conditions.IsRunning && !conditions.isDashing && conditions.isFighting;
        }
    }
    public class SwordIdleState : State
    {
        public SwordIdleState(string animationName) : base(animationName, 0) { }
        public override bool IsMatchingConditions(Conditions conditions)
        {
            return conditions.isGrounded &&
                !conditions.isAttacked &&
                conditions.isFighting &&
                conditions.moveDirection == Vector2.zero;
        }
    }
    public class IdleState : State
    {
        public IdleState(string animationName) : base(animationName, 0) { }
        public override bool IsMatchingConditions(Conditions conditions)
        {
            return conditions.isGrounded &&
                !conditions.isAttacked &&
                !conditions.isFighting &&
                conditions.moveDirection == Vector2.zero;
        }
    }
}



