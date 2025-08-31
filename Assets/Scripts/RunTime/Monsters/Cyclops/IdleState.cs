using UnityEngine;
using Cysharp.Threading.Tasks;
namespace Game.Monsters.Cyclops
{
    public class IdleState : IdleStateBase<CyclopsController>
    {
        public IdleState(CyclopsController controller) : base(controller) { }


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