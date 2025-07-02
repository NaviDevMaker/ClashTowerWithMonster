using UnityEngine;

[CreateAssetMenu]
public class ProjectileAttackMonsterStatus : MonsterStatusData
{
    [SerializeField] float projectileMoveSpeed;
    [SerializeField] GameObject mover;
    public float ProjectileMoveSpeed { get => projectileMoveSpeed;}
    public GameObject Mover { get => mover;}
}
