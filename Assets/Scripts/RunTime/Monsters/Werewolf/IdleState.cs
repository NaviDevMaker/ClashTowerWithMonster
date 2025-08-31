using UnityEngine;
using Cysharp.Threading.Tasks;
namespace Game.Monsters.Werewolf
{
    public class IdleState : IdleStateBase<WerewolfController>
    {
        public IdleState(WerewolfController controller) : base(controller) { }


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