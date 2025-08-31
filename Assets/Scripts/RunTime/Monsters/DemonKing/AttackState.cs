using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Monsters.DemonKing
{
    public class AttackState : AttackStateBase<DemonKingController>
    {
        public AttackState(DemonKingController controller) : base(controller) { }

        public override void OnEnter()
        {
            base.OnEnter();
            //This paremetars are examples,so please change it to your preference!!
            if (attackEndNomTime == 0f) StateFieldSetter.AttackStateFieldSet<DemonKingController >(controller, this, clipLength,17,
                controller.MonsterStatus.AttackInterval);
            if (controller.statusCondition.Confusion.inverval == 0f) controller.statusCondition.Confusion.inverval = 2.0f;
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
        }
        public override void OnExit()
        {
            base.OnExit();
        }
        protected override async UniTask Attack_Generic(Func<List<UnitBase>> getTargets, UnityAction<UnitBase> specialEffectAttack = null, UnityAction continuousAttack = null)
        {
            UnityAction<UnitBase> action = (target) => ConfusionTarget(target);
            await base.Attack_Generic(getTargets,action);
        }    
        async void ConfusionTarget(UnitBase target)
        {
            if (target is IPlayer) return;
            var r = UnityEngine.Random.Range(0, 100);
            var isConfusion = r <= 30;
            var statusConditionType = StatusConditionType.Confusion;
            if(isConfusion)
            {
                var visualTokens = target.statusCondition.visualTokens;
                if (visualTokens.TryGetValue(statusConditionType, out var cls))
                {
                    cls.Cancel();
                    cls.Dispose();
                    visualTokens.Remove(statusConditionType);
                }
                var newCls = new CancellationTokenSource();
                var expectedCls = newCls;
                visualTokens[statusConditionType] = newCls;
                target.statusCondition.Confusion.isActive = true;
                target.statusCondition.Confusion.isEffectedCount++;
                var hitEffectTask = EffectManager.Instance.statusConditionEffect.confusionEffect.GenerateConfusionHitEffect(target);
                await hitEffectTask;
                var visualCls = target.statusCondition.visualTokens[statusConditionType];
                if (expectedCls != visualCls) return;
                var interval = controller.statusCondition.Confusion.inverval;
                var confusionEffectTask = EffectManager.Instance.statusConditionEffect
                    .confusionEffect.GenerateConfusionEffect(target,interval,cls);
                var particle = await confusionEffectTask;
                if (target == null) return;
                target.statusCondition.Confusion.isEffectedCount--;
                var count = target.statusCondition.Confusion.isEffectedCount;
                if (count == 0) target.statusCondition.Confusion.isActive = false;
                await RelatedToParticleProcessHelper.WaitUntilParticleDisappear(particle);
            }
        }
    }
}