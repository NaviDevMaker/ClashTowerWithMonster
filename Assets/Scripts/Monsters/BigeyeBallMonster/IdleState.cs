using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

       
namespace Game.Monsters.BigEyeMonster
{
    public class IdleState : IdleStateBase<BigEyeBallMonsterController>
    {
        public IdleState(BigEyeBallMonsterController controller) : base(controller) { }

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
            Func<bool> isSummoned = (() => controller.isSummoned);
           await UniTask.WaitUntil(isSummoned);
           await base.OnEnterProcess();
        }
    }

}