using UnityEngine;

[CreateAssetMenu]
public class FlyProjectileStatusData : FlyingMonsterStatusData
{
    [SerializeField] GameObject projectileHitJudgeObj;
    public GameObject ProjectileHitJudgeObj => projectileHitJudgeObj;
}
