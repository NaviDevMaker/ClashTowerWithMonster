using UnityEngine;

[CreateAssetMenu]
public class ProjectileAttackMonsterStatus : MonsterStatusData
{
    [SerializeField] float projectileMoveSpeed;
    [SerializeField] Transform moverStartTra;
    [SerializeField] GameObject mover;
    public float ProjectileMoveSpeed { get => projectileMoveSpeed;}
    public GameObject Mover { get => mover;}
    public Transform MoverStartTra { get => moverStartTra; }
}
