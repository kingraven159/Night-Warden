using System;
using UnityEngine.Rendering;

[Serializable]
public class PlayerAnimationManager
{
    //Base
    public string Idle { get; private set; }
    public string Run {  get; private set; }
    public string Jump { get; private set; }
    public string Fall {  get; private set; }
    public string Dash { get; private set; }
    public string Slide { get; private set; }
    public string Hurt { get; private set; }
    public string Die { get; private set; }

    //Sword
    public string SwordIdle { get; private set; }
    public string SwordAttack1 { get; private set; }
    public string SwordAttack2 { get; private set; }
    public string SwordAttack3 { get; private set; }

    //Bow
    public string BowAttack1 { get; private set; }
    public string BowAttack2 { get; private set; }

    public void Initialize()
    {
        //Base
        Idle = "IdleState";
        Run = "RunState";
        Jump = "JumpState";
        Fall = "FallState";
        Dash = "DashState";
        Slide = "SlideState";
        Hurt = "HurtState";
        Die = "DieState";

        //Sword
        SwordIdle = "SwordIdleState";
        SwordAttack1 = "SwordAttack1State";
        SwordAttack2 = "SwordAttack2State";
        SwordAttack3 = "SwordAttack3State";

        //Bow
        BowAttack1 = "BowAttack1State";
        BowAttack2 = "BowAttack2State";
    }

}
