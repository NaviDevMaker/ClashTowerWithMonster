using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
namespace Game.Monsters.WormMonster
{
    public class IdleState : IdleStateBase<WormMonsterController>
    {
        public IdleState(WormMonsterController controller) : base(controller) { }


        public override void OnEnter()
        {
            OnEnterProcess().Forget();
        }
        public override void OnUpdate()
        {
           base.OnUpdate();
        }
        public override void OnExit()
        {
            base.OnExit();
        }
        protected override async UniTask OnEnterProcess()
        {
            try
            {
                var summonWaitTime = controller.MonsterStatus.SummonWaitTime;
                Func<bool> isSummoned = (() => controller.isSummoned);
                await UniTask.WaitUntil(isSummoned);
                AllResetBoolProparty();
                nextState = controller.BurrowChaseState;
                UIManager.Instance.StartSummonTimer(summonWaitTime, controller).Forget();
                await UniTask.Yield();
                controller.SummonMoveAction();
                await UniTask.Delay(TimeSpan.FromSeconds(summonWaitTime));
                isEndSummon = true;
            }
            catch (OperationCanceledException) { }
        }

    }

}