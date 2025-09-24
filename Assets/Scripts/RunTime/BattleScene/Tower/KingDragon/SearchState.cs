using System.Linq;
using UnityEngine;


namespace Game.Monsters.KingDragon
{
    public class SearchState : StateMachineBase<KingDragonController>
    {
        public SearchState (KingDragonController controller) : base (controller) { }
        public override void OnEnter() => nextState = controller.AttackState;
        public override void OnExit(){ }
        public override void OnUpdate()
        {
            var target = controller.kingDragonMethod.SetTarget(controller.AttackState.currentAttackType);
            if (target != null) ChangeToAttackState(target);
            else  controller.AttackState.ChangeCurrentAttackType(controller.AttackState.currentAttackType);
        }

        void ChangeToAttackState(UnitBase target)
        {
            controller.AttackState.targetEnemy = target;
            controller.ChangeState(nextState);
        }
    }
}

