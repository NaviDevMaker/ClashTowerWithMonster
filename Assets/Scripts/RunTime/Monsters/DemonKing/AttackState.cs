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

        ParticleSystem hitConfusionEffect;
        public override void OnEnter()
        {
            SetConfusionEffect();
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
        protected override async UniTask Attack_Generic(AttackArguments attackArguments)
        {
            var arguments = new AttackArguments
            { 
                getTargets = attackArguments.getTargets,
                attackEffectAction = PlayHitConfusionEffect,
                specialEffectAttack = ConfusionTarget
            };

            await base.Attack_Generic(arguments);
        }    
        async void ConfusionTarget(UnitBase target)
        {
            if (target is IPlayer || target is TowerController) return;
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

        //これ攻撃時の混乱エフェクトで相手が確率で混乱するときに出すエフェクトじゃないよ
        async void PlayHitConfusionEffect()
        {
            var pos = PositionGetter.GetFlatPos(controller.rangeAttackObj.transform.position);
            var rot = controller.transform.rotation;
            var originalScale = hitConfusionEffect.transform.lossyScale;
            var targetScale = new Vector3(originalScale.x * 2.5f, originalScale.y * 2.5f, originalScale.z * 3.5f);
            var effect = UnityEngine.Object.Instantiate(hitConfusionEffect, pos, rot);
            effect.transform.localScale = targetScale;
            effect.Play();
            var task = RelatedToParticleProcessHelper.WaitUntilParticleDisappear(effect);
            await task;
            if(effect != null) UnityEngine.Object.Destroy(effect.gameObject);
        }
        async void SetConfusionEffect()
        {
            var confusionObj = await SetFieldFromAssets.SetField<GameObject>("Effects/ConfusionHitEffect");
            hitConfusionEffect = confusionObj.GetComponent<ParticleSystem>();
        }
    }
}