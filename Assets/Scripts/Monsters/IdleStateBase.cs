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

        //idle�̏����̂ݓ��������炱�̃��\�b�h�̌�ɐe�N���X�ł͏��������ʂȂǂ��Ăяo��
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

