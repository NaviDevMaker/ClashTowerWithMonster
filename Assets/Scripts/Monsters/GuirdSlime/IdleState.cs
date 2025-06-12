using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Monsters.GuirdSlime
{
    public class IdleState : IdleStateBase<GuirdSlimeController>
    {
        public IdleState(GuirdSlimeController guirdSlime) : base(guirdSlime) { }


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

