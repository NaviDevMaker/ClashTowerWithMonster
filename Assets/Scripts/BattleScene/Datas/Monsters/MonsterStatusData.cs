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
    public StateAnimaSpeedInfo AnimaSpeedInfo { get => animaSpeedInfo;}
}
//攻撃の種類
public enum AttackType
{ 
   Simple,
   Long,
}

//モンスターのタイプ、攻撃するタイプかHPは高い代わりにタワーにしか
public enum MonsterAttackType
{
　ToEveryThing,
  OnlyBuilding,
}
public enum MonsterMoveType
{ 
   Walk,
   Fly,
}



