using Cysharp.Threading.Tasks;
using UnityEngine;

       
namespace Game.Monsters.SlimeKing
{
    public class IdleState : IdleStateBase<SlimeKingController>
    {
        public IdleState(SlimeKingController controller) : base(controller) { }

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