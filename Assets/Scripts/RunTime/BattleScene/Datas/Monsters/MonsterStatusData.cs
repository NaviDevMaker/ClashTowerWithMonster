using UnityEngine;

[System.Serializable]
public class StateAnimaInfo
{
    [SerializeField] float attackStateAnimSpeed;
    [SerializeField] float deathStateAnimSpeed;
    [SerializeField] AttackMotionType attackMotionType;
    public float AttackStateAnimSpeed { get => attackStateAnimSpeed;}
    public float DeathStateAnimSpeed { get => deathStateAnimSpeed;}
    public AttackMotionType AttackMotionType { get => attackMotionType;}
}

[CreateAssetMenu]

public class MonsterStatusData : StatusData
{
    [SerializeField] StateAnimaInfo animInfo;
    [SerializeField] float attackInterval;
    [SerializeField] float chaseRange;
    [SerializeField] float summonWaitTime;
    [Tooltip("���̃����X�^�[��Collider�̔��a�ȏ�(�����o��������傫��)�ɂ��Ȃ��Ɖ����o�����֌W�ŉi���ɍU�����[�h�ɓ���Ȃ����炻��������낵����")]
    [SerializeField] float attackRange;
    [SerializeField] float moveSpeed;
    [SerializeField] float moveStep;
    [SerializeField] AttackType attackType;
    [SerializeField] MonsterAttackType monsterAttackType;
    [SerializeField] MonsterMoveType monsterMoveType;
    public float AttackRange { get => attackRange; }
    public float MoveSpeed { get => moveSpeed; }
    public float ChaseRange { get => chaseRange; }
    public float MoveStep { get => moveStep; }
    public AttackType AttackType { get => attackType;}
    public MonsterAttackType MonsterAttackType { get => monsterAttackType;}
    public MonsterMoveType MonsterMoveType { get => monsterMoveType;}
    public float SummonWaitTime { get => summonWaitTime;}
    public StateAnimaInfo AnimaSpeedInfo { get => animInfo;}
    public float AttackInterval { get => attackInterval;}
}
//�U���̎��
public enum AttackType
{ 
   Simple,
   Long,
   Range,
   Continuous
}
//�����X�^�[�̃^�C�v�A�U������^�C�v��HP�͍�������Ƀ^���[�ɂ���
public enum MonsterAttackType
{
�@ RelyOnMoveType,
   ToEveryThing,
   OnlyBuilding,
}
public enum MonsterMoveType
{ 
   Walk,
   Fly,
}
public enum AttackMotionType
{ 
    Simple,
    DestractionMachine,
}




