using UnityEngine;

[CreateAssetMenu(fileName = "Player Data", menuName = "Scriptable Object/Player Data")]
public class PlayerData : ScriptableObject
{
    [Header("Gravity")]
    [HideInInspector] public float grvaityStrength;    //�߷� ��
    [HideInInspector] public float gravityScale;       //�߷� ������

    [Space(5)]
    public float fallGravityMult;   //�߶��Ҷ� �߶� �ӵ��� �� 
    public float maxFallSpeed;      //�߶��ϴ� �ְ�ӵ�
    [Space(5)]
    public float fastGravityMult;   //���� ������ �߶��ϴ� �ӵ��� ��
    public float maxFastFallSpeed;  //�߶��ӵ��� �ְ� �ӵ�

    [Space(20)]

    [Header("Run")]
    public float runMaxSpeed;      //�ְ�ӵ�
    public float runAcceleration;  //���ӵ�
    [HideInInspector]public float runAccelAmount;
    public float runDeccleration;  //-���ӵ�
    [HideInInspector]public float runDecclAmount;

    [Header("Slide")]
    public float slideSpeed;  //�����̵� �ӵ�
    public float sildeAccel;  //�����̵� ���ӵ�

    [Space(5)]
    [Range(0f, 1f)] public float wallJumpRunLerp;   //������
    [Range(0f, 1.5f)] public float wallJumpTime;    //������ �ð�
    public bool doTurnOnWallJump;                   //������ ��/����

    [Space(20)]

    [Header("Assists")]
    [Range(0.01f, 0.5f)] public float coyoteTime;           //�ڿ��� Ÿ��
    [Range(0.01f, 0.5f)] public float jumpInputBufferTime;  //���� �Է� �����ð�

    [Header("Stats")]
    public int maxHp = 5;        //�ִ� ü��
    public int currentHp = 5;    //����ü��
    public int overShield = 0;   //���� ����
    public int atk = 3;          //���ݷ�
}
