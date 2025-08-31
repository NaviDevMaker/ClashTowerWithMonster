using UnityEngine;
using Cysharp.Threading.Tasks;
namespace Game.Monsters.DemonKing
{
    public class IdleState : IdleStateBase<DemonKingController>
    {
        public IdleState(DemonKingController controller) : base(controller) { }


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