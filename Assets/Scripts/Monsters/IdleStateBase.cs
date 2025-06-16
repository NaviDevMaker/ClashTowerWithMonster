using Game.Monsters.Slime;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

namespace Game.Monsters
{
    public class IdleStateBase<T> : StateMachineBase<T> where T : MonsterControllerBase<T>
    {
        public IdleStateBase(T controler) : base(controler) { }
        bool isEndSummon = false;
        public override void OnEnter() { }
        public override void OnUpdate()
        {
            if (isEndSummon) controller.ChangeState(nextState);
        }
        public override void OnExit() { }

        //idleの処理のみ同じだからこのメソッドの後に親クラスでは召喚時効果などを呼び出す
        protected virtual async UniTask OnEnterProcess()
        {
            var summonWaitTime = controller.MonsterStatus.SummonWaitTime;
            Func<bool> isSummoned = (() => controller.isSummoned);
            await UniTask.WaitUntil(isSummoned);
            AllResetBoolProparty();
            nextState = controller.ChaseState;
            CanMoveTimerSetter.Instance.StartTimer(summonWaitTime, controller).Forget();
            await UniTask.Delay(TimeSpan.FromSeconds(summonWaitTime));
            isEndSummon = true;
        }

        void AllResetBoolProparty()
        {
            controller.animator.SetBool(controller.MonsterAnimPar.Chase, false);
            controller.animator.SetBool(controller.MonsterAnimPar.Attack, false);
        }
    }

}

