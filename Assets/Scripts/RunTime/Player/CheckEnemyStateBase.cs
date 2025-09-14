using System.Linq;
using UnityEngine;


namespace Game.Players
{
    /// <summary>
    /// 敵が範囲内にいるかどうかのチェック。
    /// </summary>
    public class CheckEnemyStateBase<T> : StateMachineBase<T> where T : PlayerControllerBase<T>
    {
        public CheckEnemyStateBase(T controller) : base(controller) { }
        UnitBase target = null;
        public override void OnEnter() 
        {
            Debug.Log("cdnsjcjkdsdskcjsdkcsddskvcdsjvjdsvdsjvcdskjcdsbjckdscbkdsj");
            nextState = controller.AttackState;
        }
        public override void OnUpdate()
        {
            Debug.Log(controller.MoveState.isPressedA);
            CheckEnemyExistInRange();
            if (target != null && controller.currentState != controller.AttackState
                && !controller.isDead)
            {
                var moveState = controller.MoveState;
                Debug.Log(moveState.isPressedA);
                if(moveState.isPressedA)
                {
                    controller.cls?.Cancel();
                    controller.cls?.Dispose();
                    controller.ChangeState(nextState);
                }  
                else if(!moveState.isMoving) controller.ChangeState(nextState);
            }
        }

        public override void OnExit() { }
        void CheckEnemyExistInRange()
        {
            var attackState = controller.AttackState;
            attackState.target = target;
            var sortedArray = SortExtention.GetSortedArrayByDistance_Sphere<UnitBase>(controller.gameObject, controller.PlayerStatus.AttackRange);
            var filterdArray = sortedArray.Where((cmp) =>
            {
                var unitSide = cmp.GetUnitSide(controller.ownerID);
                var isDead = cmp.isDead;
                var playerAttackType = controller.PlayerStatus.PlayerAttackType;
                var isTransparent = cmp.statusCondition.Transparent.isActive;
                var isNonTarget = cmp.statusCondition.NonTarget.isActive;
                if(playerAttackType == PlayerAttackType.OnlyGroundedEnemy)
                {
                    if(cmp is IMonster)
                    {
                        var unitMoveType = cmp.moveType;
                        if (unitMoveType == MoveType.Fly) return false;
                    }
                }
                return unitSide == Side.EnemySide && !isDead && !isTransparent && !isNonTarget;
            }).ToArray();

            if (filterdArray.Length == 0)
            {
                target = null;
                attackState.target = null;
                return;
            }
            var firstEnemy = filterdArray[0];
            if (target == firstEnemy) return;
            var newTarget = firstEnemy;
            target = newTarget;
            attackState.target = target;           
        }
    }

}

