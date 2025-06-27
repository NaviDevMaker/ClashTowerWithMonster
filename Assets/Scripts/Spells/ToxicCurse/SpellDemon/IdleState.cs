using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
namespace Game.Monsters.SpellDemon
{
    public class IdleState : IdleStateBase<SpellDemonController>
    {
        public IdleState(SpellDemonController controller) : base(controller) { }


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
            var waitTime = controller.MonsterStatus.SummonWaitTime;
            await UniTask.Delay(TimeSpan.FromSeconds(waitTime));
            nextState = controller.AttackState;
            isEndSummon = true;
        }
        protected override void AllResetBoolProparty() => controller.animator.SetBool(controller.MonsterAnimPar.Attack, false);

    }

}