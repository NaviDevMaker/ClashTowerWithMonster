using UnityEngine;

[CreateAssetMenu]
public class PlayerStatusData : StatusData
{
   
    [SerializeField] float attackRange;
    [SerializeField] float moveSpeed;
    [SerializeField] PlayerAttackType attackType;
    
    public float AttackRange { get => attackRange; }
    public float MoveSpeed { get => moveSpeed; }
    public PlayerAttackType PlayerAttackType { get => attackType;}
}

public enum PlayerAttackType
{
  OnlyGroundedEnemy,
  Everything,
}
