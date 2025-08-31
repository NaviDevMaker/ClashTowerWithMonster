using UnityEngine;
using Cysharp.Threading.Tasks;
namespace Game.Monsters.FylingDemon
{
    public class IdleState : IdleStateBase<FylingDemonController>
    {
        public IdleState(FylingDemonController controller) : base(controller) { }


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