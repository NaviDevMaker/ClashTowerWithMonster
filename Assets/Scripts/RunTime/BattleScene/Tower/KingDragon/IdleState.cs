using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace Game.Monsters.KingDragon
{
    public class IdleState : StateMachineBase<KingDragonController>
    {
        public IdleState(KingDragonController controller) : base(controller) { }
        bool isEndWaitAction = false;
        public override async void OnEnter()
        {
            nextState = controller.SearchState;
            controller.IsInvincible = true;
            controller.statusCondition.NonTarget.isActive = true;
            controller.animator.speed = 0f;

            try
            {
                await WaitMove();
            }
            catch (OperationCanceledException) { return; }
        }

        public override void OnExit()
        {
        }

        public override void OnUpdate()
        {
            if (isEndWaitAction)
            {
                Debug.Log("“®‚«o‚µ‚Ü‚·");
                controller.ChangeState(nextState);
            }
        }
        async UniTask WaitMove()
        {
            try
            {
                await UniTask.WaitUntil(() => controller.IsDestroyedAllTower()
                        , cancellationToken: controller.GetCancellationTokenOnDestroy());
            }
            catch (OperationCanceledException) {throw;}
            controller.animator.speed = 1.0f;
            controller.IsInvincible = false;
            controller.statusCondition.NonTarget.isActive = false;
            isEndWaitAction = true;
        }
    }
}

