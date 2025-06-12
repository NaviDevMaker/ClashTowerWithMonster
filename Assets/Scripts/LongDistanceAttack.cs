using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;

public class LongDistanceAttack<T> : MonoBehaviour where T : UnitBase
{

    public bool IsReachedTargetPos { get; set; } = false;
    public UnitBase target { get; set; }
    [SerializeField] protected ParticleSystem hitEffect;
    public T attacker { get; private set; }
    protected Coroutine moveCoroutine;
    public UnityAction<LongDistanceAttack<T>> OnEndProcess;
    public virtual void Initialize(T attacker) 
    {
        this.attacker = attacker;
    }

    protected virtual void Update() { }
    protected virtual void DamageToEnemy(UnitBase target,Func<UniTask> hitEffect = null)
    {
        if (target.isDead || target == null) OnEndProcess?.Invoke(this);
        else 
        {
           if(target.TryGetComponent(out IUnitDamagable unitDamagable))
           {
                if (hitEffect != null) hitEffect().Forget();
                unitDamagable.Damage(attacker.StatusData.AttackAmount);
                OnEndProcess?.Invoke(this);
            }
        }
    }
}
