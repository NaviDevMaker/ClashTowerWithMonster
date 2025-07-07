using UnityEngine;

namespace Game.Players.Sword
{
    public class AttackState : AttackStateBase<SwordPlayerController>
    {
        public AttackState(SwordPlayerController controller) : base(controller) { }
        public override void OnEnter()
        {
            if(interval == 0f) interval = 0.8f;
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

