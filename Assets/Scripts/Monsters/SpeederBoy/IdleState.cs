using UnityEngine;
using Cysharp.Threading.Tasks;
namespace Game.Monsters.SpeederBoy
{
    public class IdleState : IdleStateBase<SpeederBoyController>
    {
        public IdleState(SpeederBoyController controller) : base(controller) { }


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