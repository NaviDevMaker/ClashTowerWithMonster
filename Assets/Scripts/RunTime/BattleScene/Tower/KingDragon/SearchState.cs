using System.Linq;
using UnityEngine;


namespace Game.Monsters.KingDragon
{
    public class SearchState : StateMachineBase<KingDragonController>
    {
        public SearchState (KingDragonController controller) : base (controller) { }

        public override void OnEnter() => nextState = controller.AttackState;
        public override void OnExit(){ }
        public override void OnUpdate() => SetTarget();

        void SetTarget()
        {
            var sortedList = SortExtention.GetSortedArrayByDistance_Sphere<UnitBase>
                (controller.gameObject, controller.KingDragonStatus.SearchRadius).ToList();

            var filterdList = sortedList.Where(unit =>
            {
                if (unit is ISummonbable summonbable && !summonbable.isSummoned) return false;
                if(unit is IInvincible invincible && invincible.IsInvincible) return false;
                var isDead = unit.isDead;
                var side = unit.GetUnitSide(controller.ownerID);
                var isTransparent = unit.statusCondition.Transparent.isActive;
                var isNonTarget = unit.statusCondition.NonTarget.isActive;
                if (isDead || side == Side.PlayerSide || isTransparent || isNonTarget) return false;
                return true;
            }).ToList();
            if (filterdList.Count == 0) return;
            else
            {
                var target = filterdList[0];
                ChangeToAttackState(target);
            }
        }
        void ChangeToAttackState(UnitBase target)
        {
            controller.AttackState.targetEnemy = target;
            controller.ChangeState(nextState);
        }
    }
}

