using UnityEngine;

namespace Game.Players.Sword
{
    public class IdleState : IdleStateBase<SwordPlayerController>
    {
        public IdleState(SwordPlayerController contoller) : base(contoller) { }

        public override void OnEnter()
        {
            base.OnEnter();
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
        }
        public override void OnExit()
        {
            base.OnExit();
        }
    }
}

