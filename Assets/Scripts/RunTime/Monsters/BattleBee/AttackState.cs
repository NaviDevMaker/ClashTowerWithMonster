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
        protected override async UniTask Attack_Generic(Func<List<UnitBase>> getTargets,
            UnityAction<UnitBase> paresisAttack, UnityAction continuousAttack = null)
        {
            UnityAction<UnitBase> action = (currentTarget) => controller.ParesisTarget(currentTarget);
            Func<List<UnitBase>> action2 = (() => target != null ? new List<UnitBase> {target} : Enumerable.Empty<UnitBase>().ToList());
            await base.Attack_Generic(action2, action);
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