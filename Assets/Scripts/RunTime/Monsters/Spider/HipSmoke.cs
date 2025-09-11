using Game.Monsters.Spider;
using UnityEngine;

public class HipSmoke : MonoBehaviour,IRangeAttack
{
    public GameObject rangeAttackObj { get => throw new System.NotImplementedException();
                                       set => throw new System.NotImplementedException(); }

    public SpiderController attacker { get; set;}
    int damage = 2;
    public void DamageInRangeUnit()
    {
        var currentTargets =  this.GetUnitInSpecificRangeItem(attacker).Invoke();
        currentTargets.ForEach(target =>
        {
            if (target == null || attacker.isDead || this == null) return;
            if(target.TryGetComponent<IUnitDamagable>(out var unitDamagable))
            {
                unitDamagable.Damage(damage);
            }
        });
    }

    public void SetHitJudgementObject()
    {
        throw new System.NotImplementedException();
    }
}
