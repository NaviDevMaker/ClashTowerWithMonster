using UnityEngine;
using Cysharp.Threading.Tasks;
namespace Game.Monsters.LizardWarrior
{
    public class IdleState : IdleStateBase<LizardWarriorController>
    {
        public IdleState(LizardWarriorController controller) : base(controller) { }


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
            await base.OnEnterProcess();
        }

    }

}