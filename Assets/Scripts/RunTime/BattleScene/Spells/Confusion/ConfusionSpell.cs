using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;


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
        protected override void SetDuration() => spellDuration = _SpellStatus.SpellDuration;


        protected override void SetRange()
        {
            scaleAmount = 1.0f;
            base.SetRange();
        }
        protected override async UniTaskVoid Spell()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(spellDuration));
            addForceToUnit.KeepDistance(moveType);
            var units =  spellEffectHelper.GetUnitInRange();
            var filteredList = units.Where(unit =>
            {
                var inRange = spellEffectHelper.CompareUnitInRange(unit);
                var isNotTower = !(unit is ITower);
                return isNotTower && inRange;
            }).ToList();

            //一回掛けられてる状態でまたかけられた場合、前回のエフェクトは消して（一瞬頭上のクルクルは消える）またかけなおす
            var expectedClses = new List<CancellationTokenSource>();
            var tasks = new List<UniTask>();

            for (var  i = 0; i < filteredList.Count;i++)
            {          
                var unit = filteredList[i];
                if (unit == null) return;
                var visualTokens = unit.statusCondition.visualTokens;
                if (visualTokens.TryGetValue(statusConditionType, out var cls))
                {
                    cls.Cancel();
                    cls.Dispose();
                    visualTokens.Remove(statusConditionType);
                }
                var newCls = new CancellationTokenSource();
                expectedClses.Add(newCls);
                visualTokens[statusConditionType] = newCls;
                unit.statusCondition.Confusion.isActive = true;
                unit.statusCondition.Confusion.isEffectedCount++;
                var task = EffectManager.Instance.statusConditionEffect.confusionEffect.GenerateConfusionHitEffect(unit);
                tasks.Add(task);
            }
           
            await UniTask.WhenAll(tasks);
            var newTasks = new List<UniTask<ParticleSystem>>();
            for (var i = 0; i < filteredList.Count; i++)
            {
                var unit = filteredList[i];
                if (unit == null) return;
                var expectedCls = expectedClses[i];
                var cls = unit.statusCondition.visualTokens[statusConditionType];
                if (expectedCls != cls) continue;
                var task = EffectManager.Instance.statusConditionEffect.confusionEffect.GenerateConfusionEffect(unit, spellDuration, cls);
                newTasks.Add(task);
            }
            particles = await UniTask.WhenAll(newTasks);
            filteredList.ForEach(unit =>
            {
                if (unit == null) return;
                unit.statusCondition.Confusion.isEffectedCount--;
                var count = unit.statusCondition.Confusion.isEffectedCount;
                if (count == 0) unit.statusCondition.Confusion.isActive = false;
            });
            DestroyAll();
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
            if (this == null) return;  
            Destroy(this.gameObject);
        }
    }
}
