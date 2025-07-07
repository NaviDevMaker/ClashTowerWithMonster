using UnityEngine;
using Cysharp.Threading.Tasks;
namespace Game.Monsters.DestructionMachine
{
    public class IdleState : IdleStateBase<DestructionMachineController>
    {
        public IdleState(DestructionMachineController controller) : base(controller) { }


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