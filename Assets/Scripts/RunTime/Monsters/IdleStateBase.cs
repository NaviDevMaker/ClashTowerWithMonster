using Game.Monsters.Slime;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using DG.Tweening;
namespace Game.Monsters
{
    public class IdleStateBase<T> : StateMachineBase<T> where T : MonsterControllerBase<T>
    {
        public IdleStateBase(T controler) : base(controler) { }
        public bool isEndSummon = false;
        public override void OnEnter() { }
        public override void OnUpdate()
        {
            if (isEndSummon) controller.ChangeState(nextState);
        }
        public override void OnExit() { }

        //idle�̏����̂ݓ��������炱�̃��\�b�h�̌�ɐe�N���X�ł͏��������ʂȂǂ��Ăяo��
        protected virtual async UniTask OnEnterProcess()
        {
            try
            {
                var summonWaitTime = controller.MonsterStatus.SummonWaitTime;
                Func<bool> isSummoned = (() => controller.isSummoned);
                await UniTask.WaitUntil(isSummoned);
                AllResetBoolProparty();
                nextState = controller.ChaseState;
                UIManager.Instance.StartSummonTimer(summonWaitTime, controller).Forget();
                await UniTask.Yield();
                controller.SummonMoveAction();
                await UniTask.Delay(TimeSpan.FromSeconds(summonWaitTime));
                isEndSummon = true;
            }
            catch(OperationCanceledException) {}
        }

        protected virtual void AllResetBoolProparty()
        {
            controller.animator.SetBool(controller.MonsterAnimPar.Chase_Hash, false);
            controller.animator.SetBool(controller.MonsterAnimPar.Attack_Hash, false);
        }
    }

}

