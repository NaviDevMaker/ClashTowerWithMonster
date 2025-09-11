
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
namespace Game.Monsters.Salamander
{
    public class SummonFireLing : PushableWeponBase<SalamanderController>,IRangeWeponAddForce<SalamanderController>
        ,IRangeAttack
    {
        public AddForceToUnit<PushableWeponBase<SalamanderController>> _addForceToUnit { get; private set;}
        public GameObject rangeAttackObj 
        { get => throw new System.NotImplementedException();
          set => throw new System.NotImplementedException(); }

        public override void Initialize(SalamanderController owner)
        {
            ownerID = owner.ownerID;
            moveType = owner.moveType;
            UnitScale = owner.UnitScale;
            base.Initialize(owner);
            var pushAmount = 1.0f;
            var pushDuration = 0.5f;
            _addForceToUnit = new AddForceToUnit<PushableWeponBase<SalamanderController>>(this, pushAmount, pushDuration);
        }
        public void PushUnit(UnitBase target) => _addForceToUnit.CompareEachUnit(target);
        public async void DamageAndPush()
        {
            var particles = GetComponentsInChildren<ParticleSystem>().ToList();
            particles.ForEach(p => p.Play());
            var currentTargets = this.GetUnitInSpecificRangeItem(weponOwner).Invoke();
            var damage = weponOwner.MonsterStatus.AttackAmount * 2;
            currentTargets.ForEach(target =>
            {
                if(target.TryGetComponent<IUnitDamagable>(out var unitDamagable))
                {
                    unitDamagable.Damage(damage);
                    PushUnit(target);
                }
            });
            var tasks = particles.Select(p => RelatedToParticleProcessHelper.WaitUntilParticleDisappear(p)).ToArray();
            await tasks;
            particles.ForEach(p =>
            {
                if (p != null) Destroy(p.gameObject);
            });
        }
        public void SetHitJudgementObject(){ throw new System.NotImplementedException();}
    }
}


