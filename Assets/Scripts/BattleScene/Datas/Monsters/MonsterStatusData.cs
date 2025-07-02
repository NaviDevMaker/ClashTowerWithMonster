using UnityEngine;

[System.Serializable]
public class StateAnimaSpeedInfo
{
    [SerializeField] float attackStateAnimSpeed;
    [SerializeField] float deathStateAnimSpeed;
    public float AttackStateAnimSpeed { get => attackStateAnimSpeed;}
    public float DeathStateAnimSpeed { get => deathStateAnimSpeed;}
}

[CreateAssetMenu]

public class MonsterStatusData : StatusData
{
    [SerializeField] StateAnimaSpeedInfo animaSpeedInfo;
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
    public StateAnimaSpeedInfo AnimaSpeedInfo { get => animaSpeedInfo;}
}
//�U���̎��
public enum AttackType
{ 
   Simple,
   Long,
}

//�����X�^�[�̃^�C�v�A�U������^�C�v��HP�͍�������Ƀ^���[�ɂ���
public enum MonsterAttackType
{
�@ToEveryThing,
  OnlyBuilding,
}
public enum MonsterMoveType
{ 
   Walk,
   Fly,
}



