using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Monsters.BattleBee
{
    public class AttackState : AttackStateBase<BattleBeeController>
    {
        public AttackState(BattleBeeController controller) : base(controller) { }
        public override void OnEnter()
        {
            base.OnEnter();
            //This paremetars are examples,so please change it to your preference!!
            if (attackEndNomTime == 0f) StateFieldSetter.AttackStateFieldSet<BattleBeeController>(controller, this, clipLength,15,
                controller.MonsterStatus.AttackInterval);
            if (controller.statusCondition.Paresis.inverval == 0f) controller.statusCondition.Paresis.inverval = controller.MonsterStatus.AttackInterval;
        }
        protected override async UniTask Attack_Generic(SimpleAttackArguments attackArguments)
        { 
            var arguments = new SimpleAttackArguments
            {
                getTargets = attackArguments.getTargets,
                specialEffectAttack = (currentTarget) => controller.ParesisTarget(currentTarget)
            };
            await base.Attack_Generic(arguments);
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
        }
        public override void OnExit()
        {
            base.OnExit();
        }
    }
}