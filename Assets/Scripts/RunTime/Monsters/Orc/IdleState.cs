using UnityEngine;
using Cysharp.Threading.Tasks;
namespace Game.Monsters.Orc
{
    public class IdleState : IdleStateBase<OrcController>
    {
        public IdleState(OrcController controller) : base(controller) { }


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