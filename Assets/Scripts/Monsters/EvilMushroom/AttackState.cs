using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace Game.Monsters.EvilMushroom
{
    public class AttackState : AttackStateBase<EvilMushroomController>
    {
        public AttackState(EvilMushroomController controller) : base(controller) { }

        public override void OnEnter()
        {
            base.OnEnter();
            //This paremetars are examples,so please change it to your preference!!
            if(attackEndNomTime == 0f) StateFieldSetter.AttackStateFieldSet<EvilMushroomController>(controller, this, clipLength, 15, 1.5f);
            if (controller.statusCondition.Paresis.inverval == 0f) controller.statusCondition.Paresis.inverval = 1.5f;
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
        }
        public override void OnExit()
        {
            base.OnExit();
        }

        protected override async UniTask Attack_Simple()
        {
            controller.animator.speed = 1.0f;
            Debug.Log(target.gameObject.name);
            //var animDuration = clipLength * animationSpeed;
            var startNormalizeTime = controller.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            Func<bool> wait = (() =>
            {
                var now = controller.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                return now - startNormalizeTime >= attackEndNomTime;
            });
            Func<bool> waitEnd = (() =>
            {
                var now = controller.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                return now - startNormalizeTime >= 1.0f;
            });
            await UniTask.WaitUntil(wait, cancellationToken: cts.Token);//,);
            if (target != null && target.TryGetComponent<IUnitDamagable>(out var unitDamagable))
            {
                Debug.Log($"{controller.gameObject.name}のアタック");
                unitDamagable.Damage(controller.MonsterStatus.AttackAmount);
                ParesisTarget();
                EffectManager.Instance.hitEffect.GenerateHitEffect(target);
            }
            await UniTask.WaitUntil(waitEnd, cancellationToken: cts.Token);
            controller.animator.speed = 0f;
            await UniTask.Delay(TimeSpan.FromSeconds(interval), cancellationToken: cts.Token);
            isAttacking = false;
            Debug.Log("dsacdsscsad");
        }

        async void ParesisTarget()
        {
            var statusCondition = target.statusCondition;
            if (statusCondition != null)
            {
                Debug.Log("麻痺させます");
               
                var interval = controller.statusCondition.Paresis.inverval;
                statusCondition.Paresis.isActive = true;
                var attackedCount = statusCondition.Paresis.isAttackedCount;
                EffectManager.Instance.statusConditionEffect.paresisEffect.GenerateParesisEffect(target, attackCount: attackedCount);
                attackedCount++;
                await UniTask.Delay(TimeSpan.FromSeconds(interval));
                attackedCount--;
                if(attackedCount == 0) statusCondition.Paresis.isActive = false;
            }
        }
    }

}