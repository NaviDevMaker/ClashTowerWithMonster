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
    [Tooltip("このモンスターのColliderの半径以上(押し出し判定より大きい)にしないと押し出される関係で永遠に攻撃モードに入らないからそこだけよろしく俺")]
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
//攻撃の種類
public enum AttackType
{ 
   Simple,
   Long,
   Range,
   Continuous
}
//モンスターのタイプ、攻撃するタイプかHPは高い代わりにタワーにしか
public enum MonsterAttackType
{
　 RelyOnMoveType,
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




