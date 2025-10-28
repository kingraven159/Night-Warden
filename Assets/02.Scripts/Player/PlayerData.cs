using System.Runtime.CompilerServices;
using UnityEngine;

[CreateAssetMenu(fileName = "Player Data", menuName = "Scriptable Object/Player Data")]
public class PlayerData : ScriptableObject
{
    [Header("Gravity")]
    [HideInInspector] public float gravityStrength;    //중력 힘
    [HideInInspector] public float gravityScale;       //중력 스케일

    [Space(5)]
    public float fallGravityMult;   //추락할때 추락 속도의 곱 
    public float maxFallSpeed;      //추락하는 최고속도
    [Space(5)]
    public float fastFallGravityMult;   //더욱 빠르게 추락하는 속도의 곱
    public float maxFastFallSpeed;  //추락속도의 최고 속도

    [Space(20)]

    [Header("Run")]
    public float runMaxSpeed;                      //최고속도
    public float runAcceleration;                  //가속
    [HideInInspector]public float runAccelAmount;  
    public float runDecceleration;                 //감속
    [HideInInspector]public float runDeccelAmount;
    [Space(5)]
    [Range(0f, 1)] public float accelInAir;        //공중에서 가속
    [Range(0f, 1)] public float deccelInAir;       //공중에서 감속
    [Space(5)]
    public bool doConserveMomantum = true;         //운동량 보존

    [Space(20)]

    [Header("Jump")] 
    public float jumpHeight;                  //점프 높이
    public float jumpTimeToApex;              //점프 최고점까지의 시간
    [HideInInspector] public float jumpForce; //점프 힘

    [Header("Double Jump")]
    public float jumpCutGravityMult;                      //추락으로 인해 중력이 증가할 때 점프시 중력을 초기화
    [Range(0f, 1f)] public float jumpHangGravityMult; //점프 
    public float jumpHangTimeThreshold;               //점프 한계점
    [Space(5)]
    public float jumpHangAccelerationMult;            //
    public float jumpHangMaxSpeedMult;

    [Space(20)]

    [Header("Wall Jump")]
    public Vector2 wallJumpForce; //벽 점프 힘
    [Space(5)]
    [Range(0f, 1f)] public float wallJumpRunLerp;   //벽점프
    [Range(0f, 1.5f)] public float wallJumpTime;    //벽점프 시간
    public bool doTurnOnWallJump;                   //벽점프 온/오프

    [Space(20)]

    [Header("Slide")]
    public float slideSpeed;  //슬라이드 속도
    public float slideAccel;  //슬라이드 가속도


    [Space(20)]

    [Header("Assists")]
    [Range(0.01f, 0.5f)] public float coyoteTime;           //코요테 타임
    [Range(0.01f, 0.5f)] public float jumpInputBufferTime;  //점프 인풋 딜레이 시간

    [Space(20)]

    [Header("Dash")]
    public int dashAmount;  
    public float dashSpeed;   
    public float dashFreezeTime;   //대쉬 시 경직 시간
    [Space(5)]
    public float dashAttackTime;
    [Space(5)]
    public float dashEndTime;     //대쉬 끝나는 시간
    public Vector2 dashEndSpeed;  //대쉬 끝날때 속도
    [Range(0f, 1f)] public float dashEndRunLerp; //대쉬 속도에 영향을 주는 값
    [Space(5)]
    public float dashRefillTime;
    [Space(5)]
    [Range(0.01f, 0.5f)] public float dashIputBufferTime; //대쉬 인풋에 딜레이 시간

    [Header("Stats")]
    public int maxHp = 5;        //최대 체력
    public int currentHp = 5;    //현재체력
    public int overShield = 0;   //오버 쉴드
    public int atk = 3;          //공격력

    //Unity Callback, called when the inspector updates
    private void OnValidate()
    {
        //중력 힘을 계산 -(중력 힘 = 2 * 점프 높이) / (점프 최고점까지의 시간^2)
        gravityStrength = -(2 * jumpHeight) / (jumpTimeToApex * jumpTimeToApex);

        //중력 스케일을 계산 ( 중력 스케일 = 중력 힘 / 물리 중력값)
        gravityScale = gravityStrength / Physics2D.gravity.y;

        //달리기 가속도의 증감속도를 계산 (가속도 = 50* 가속도/ 최고속도)
        runAccelAmount = (50 * runAcceleration) / runMaxSpeed;
        runDeccelAmount = (50 * runDecceleration) / runMaxSpeed;

        //점프 힘을 계산 (점프 힘 = 중력 힘 * 점프 최고점까지의 시간
        jumpForce = Mathf.Abs(gravityStrength) * jumpTimeToApex;

        //달리기 가속도 증감
        runAcceleration = Mathf.Clamp(runAcceleration, 0.01f, runMaxSpeed);
        runDecceleration = Mathf.Clamp(runDecceleration, 0.01f, runMaxSpeed);
    }
}
