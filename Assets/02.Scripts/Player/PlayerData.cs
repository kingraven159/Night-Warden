using System.Runtime.CompilerServices;
using UnityEngine;

[CreateAssetMenu(fileName = "Player Data", menuName = "Scriptable Object/Player Data")]
public class PlayerData : ScriptableObject
{
    [Header("Gravity")]
    [HideInInspector] public float gravityStrength;    //�߷� ��
    [HideInInspector] public float gravityScale;       //�߷� ������

    [Space(5)]
    public float fallGravityMult;   //�߶��Ҷ� �߶� �ӵ��� �� 
    public float maxFallSpeed;      //�߶��ϴ� �ְ�ӵ�
    [Space(5)]
    public float fastFallGravityMult;   //���� ������ �߶��ϴ� �ӵ��� ��
    public float maxFastFallSpeed;  //�߶��ӵ��� �ְ� �ӵ�

    [Space(20)]

    [Header("Run")]
    public float runMaxSpeed;                      //�ְ�ӵ�
    public float runAcceleration;                  //����
    [HideInInspector]public float runAccelAmount;  
    public float runDecceleration;                 //����
    [HideInInspector]public float runDeccelAmount;
    [Space(5)]
    [Range(0f, 1)] public float accelInAir;        //���߿��� ����
    [Range(0f, 1)] public float deccelInAir;       //���߿��� ����
    [Space(5)]
    public bool doConserveMomantum = true;         //��� ����

    [Space(20)]

    [Header("Jump")] 
    public float jumpHeight;                  //���� ����
    public float jumpTimeToApex;              //���� �ְ��������� �ð�
    [HideInInspector] public float jumpForce; //���� ��

    [Header("Double Jump")]
    public float jumpCutGravityMult;                      //�߶����� ���� �߷��� ������ �� ������ �߷��� �ʱ�ȭ
    [Range(0f, 1f)] public float jumpHangGravityMult; //���� 
    public float jumpHangTimeThreshold;               //���� �Ѱ���
    [Space(5)]
    public float jumpHangAccelerationMult;            //
    public float jumpHangMaxSpeedMult;

    [Space(20)]

    [Header("Wall Jump")]
    public Vector2 wallJumpForce; //�� ���� ��
    [Space(5)]
    [Range(0f, 1f)] public float wallJumpRunLerp;   //������
    [Range(0f, 1.5f)] public float wallJumpTime;    //������ �ð�
    public bool doTurnOnWallJump;                   //������ ��/����

    [Space(20)]

    [Header("Slide")]
    public float slideSpeed;  //�����̵� �ӵ�
    public float slideAccel;  //�����̵� ���ӵ�


    [Space(20)]

    [Header("Assists")]
    [Range(0.01f, 0.5f)] public float coyoteTime;           //�ڿ��� Ÿ��
    [Range(0.01f, 0.5f)] public float jumpInputBufferTime;  //���� ��ǲ ������ �ð�

    [Space(20)]

    [Header("Dash")]
    public int dashAmount;  
    public float dashSpeed;   
    public float dashFreezeTime;   //�뽬 �� ���� �ð�
    [Space(5)]
    public float dashAttackTime;
    [Space(5)]
    public float dashEndTime;     //�뽬 ������ �ð�
    public Vector2 dashEndSpeed;  //�뽬 ������ �ӵ�
    [Range(0f, 1f)] public float dashEndRunLerp; //�뽬 �ӵ��� ������ �ִ� ��
    [Space(5)]
    public float dashRefillTime;
    [Space(5)]
    [Range(0.01f, 0.5f)] public float dashIputBufferTime; //�뽬 ��ǲ�� ������ �ð�

    [Header("Stats")]
    public int maxHp = 5;        //�ִ� ü��
    public int currentHp = 5;    //����ü��
    public int overShield = 0;   //���� ����
    public int atk = 3;          //���ݷ�

    //Unity Callback, called when the inspector updates
    private void OnValidate()
    {
        //�߷� ���� ��� -(�߷� �� = 2 * ���� ����) / (���� �ְ��������� �ð�^2)
        gravityStrength = -(2 * jumpHeight) / (jumpTimeToApex * jumpTimeToApex);

        //�߷� �������� ��� ( �߷� ������ = �߷� �� / ���� �߷°�)
        gravityScale = gravityStrength / Physics2D.gravity.y;

        //�޸��� ���ӵ��� �����ӵ��� ��� (���ӵ� = 50* ���ӵ�/ �ְ�ӵ�)
        runAccelAmount = (50 * runAcceleration) / runMaxSpeed;
        runDeccelAmount = (50 * runDecceleration) / runMaxSpeed;

        //���� ���� ��� (���� �� = �߷� �� * ���� �ְ��������� �ð�
        jumpForce = Mathf.Abs(gravityStrength) * jumpTimeToApex;

        //�޸��� ���ӵ� ����
        runAcceleration = Mathf.Clamp(runAcceleration, 0.01f, runMaxSpeed);
        runDecceleration = Mathf.Clamp(runDecceleration, 0.01f, runMaxSpeed);
    }
}
