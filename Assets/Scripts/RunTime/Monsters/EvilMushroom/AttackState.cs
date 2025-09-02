using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Monsters.EvilMushroom
{
    public class AttackState : AttackStateBase<EvilMushroomController>
    {
        public AttackState(EvilMushroomController controller) : base(controller) { }

        public override void OnEnter()
        {
            base.OnEnter();
            //This paremetars are examples,so please change it to your preference!!
            if(attackEndNomTime == 0f) StateFieldSetter.AttackStateFieldSet<EvilMushroomController>(controller, this, clipLength, 15,
                controller.MonsterStatus.AttackInterval);
            if (controller.statusCondition.Paresis.inverval == 0f) controller.statusCondition.Paresis.inverval = controller.MonsterStatus.AttackInterval;
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
                specialEffectAttack = (currentTarget) => controller.ParesisTarget(currentTarget)
            };
            await base.Attack_Generic(attackArguments);
        }

        //async void ParesisTarget()
        //{
        //    if (target == null || target is TowerControlller) return;
        //    var statusCondition = target.statusCondition;
        //    if (statusCondition != null)
        //    {
        //        Debug.Log("–ƒáƒ‚³‚¹‚Ü‚·");
               
        //        var interval = controller.statusCondition.Paresis.inverval;
        //        statusCondition.Paresis.isActive = true;
        //        var attackedCount = statusCondition.Paresis.isEffectedCount;
        //        EffectManager.Instance.statusConditionEffect.paresisEffect.GenerateParesisEffect(target, attackCount: attackedCount);
        //        statusCondition.Paresis.isEffectedCount++;
        //        await UniTask.Delay(TimeSpan.FromSeconds(interval));
        //        statusCondition.Paresis.isEffectedCount--;
        //        if(statusCondition.Paresis.isEffectedCount == 0) statusCondition.Paresis.isActive = false;
        //    }
        //}
    }

}