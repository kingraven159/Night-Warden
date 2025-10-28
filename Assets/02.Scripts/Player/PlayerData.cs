using UnityEngine;

[CreateAssetMenu(fileName = "Player Data", menuName = "Scriptable Object/Player Data")]
public class PlayerData : ScriptableObject
{
    [Header("Gravity")]
    [HideInInspector] public float grvaityStrength;    //중력 힘
    [HideInInspector] public float gravityScale;       //중력 스케일

    [Space(5)]
    public float fallGravityMult;   //추락할때 추락 속도의 곱 
    public float maxFallSpeed;      //추락하는 최고속도
    [Space(5)]
    public float fastGravityMult;   //더욱 빠르게 추락하는 속도의 곱
    public float maxFastFallSpeed;  //추락속도의 최고 속도

    [Space(20)]

    [Header("Run")]
    public float runMaxSpeed;      //최고속도
    public float runAcceleration;  //가속도
    [HideInInspector]public float runAccelAmount;
    public float runDeccleration;  //-가속도
    [HideInInspector]public float runDecclAmount;

    [Header("Slide")]
    public float slideSpeed;  //슬라이드 속도
    public float sildeAccel;  //슬라이드 가속도

    [Space(5)]
    [Range(0f, 1f)] public float wallJumpRunLerp;   //벽점프
    [Range(0f, 1.5f)] public float wallJumpTime;    //벽점프 시간
    public bool doTurnOnWallJump;                   //벽점프 온/오프

    [Space(20)]

    [Header("Assists")]
    [Range(0.01f, 0.5f)] public float coyoteTime;           //코요테 타임
    [Range(0.01f, 0.5f)] public float jumpInputBufferTime;  //점프 입력 보정시간

    [Header("Stats")]
    public int maxHp = 5;        //최대 체력
    public int currentHp = 5;    //현재체력
    public int overShield = 0;   //오버 쉴드
    public int atk = 3;          //공격력
}
