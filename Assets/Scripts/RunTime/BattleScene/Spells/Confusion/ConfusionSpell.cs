using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;


namespace Game.Spells.Confusion
{
    public class ConfusionSpell : SpellBase
    {
        readonly StatusConditionType statusConditionType = StatusConditionType.Confusion;
        ParticleSystem[] particles = new ParticleSystem[0];
        protected override async void Initialize()
        {
            _SpellStatus = await SetFieldFromAssets.SetField<SpellStatus>("Datas/Spells/Confusion");
            pushEffectUnit = PushEffectUnit.OnlyEnemyUnit;
            var pushAmount = _SpellStatus.PushAmount;
            var perPushDuration = _SpellStatus.PerPushDurationAndStunTime;
            addForceToUnit = new AddForceToUnit<SpellBase>(this,pushAmount,perPushDuration,pushEffectUnit);
            base.Initialize();
        }
        protected override void SetDuration() => spellDuration = 3f;


        protected override void SetRange()
        {
            scaleAmount = 1.0f;
            base.SetRange();
        }
        protected override async UniTaskVoid Spell()
        {
            addForceToUnit.KeepDistance(moveType);
            var units =  spellEffectHelper.GetUnitInRange();
            var filteredList = units.Where(unit =>
            {
                var inRange = spellEffectHelper.CompareUnitInRange(unit);
                var isNotTower = !(unit is TowerControlller);
                return isNotTower && inRange;
            }).ToList();

            var tasks = new List<UniTask>();
            filteredList.ForEach(unit =>
            {
                if (unit == null) return;
                var visualTokens = unit.statusCondition.visualTokens;
                if (visualTokens.TryGetValue(statusConditionType,out var cls))
                {
                    if (cls == null) return;
                    cls.Cancel();
                    cls.Dispose();
                }
                var newCls = new CancellationTokenSource();
                visualTokens[statusConditionType] = newCls;
                unit.statusCondition.Confusion.isActive = true;
                unit.statusCondition.Confusion.isEffectedCount++;
                var task = EffectManager.Instance.statusConditionEffect.confusionEffect.GenerateConfusionHitEffect(unit);
                tasks.Add(task);
            });
            await UniTask.WhenAll(tasks);
            var newTasks = new List<UniTask<ParticleSystem>>();
            filteredList.ForEach(unit =>
            {
                if (unit == null) return;
                var cls = unit.statusCondition.visualTokens[statusConditionType];
                var task = EffectManager.Instance.statusConditionEffect.confusionEffect.GenerateConfusionEffect(unit, spellDuration,cls);
                newTasks.Add(task);
            });
            particles = await UniTask.WhenAll(newTasks);
            filteredList.ForEach(unit =>
            {
                if (unit == null) return;
                unit.statusCondition.Confusion.isEffectedCount--;
                var count = unit.statusCondition.Confusion.isEffectedCount;
                if (count == 0) unit.statusCondition.Confusion.isActive = false;
            });
            //DestroyAll();
        }
        protected override async void DestroyAll()
        {
            var waitTasks = new List<UniTask>();
            particles.ToList().ForEach(p =>
            {
               if (p == null) return;
               var task = RelatedToParticleProcessHelper.WaitUntilParticleDisappear(p);
               waitTasks.Add(task);
            });
            await UniTask.WhenAll(waitTasks);
            Destroy(this.gameObject);
        }
    }
}
