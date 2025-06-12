using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Monsters.Slime
{
    public class IdleState : IdleStateBase<SlimeController>
    {
        public IdleState(SlimeController slime) : base(slime) { }


        public override void OnEnter()
        {
            Debug.Log("idle‚Å‚·");
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

