using UnityEngine;

namespace Game.Monsters.AttackerBoy
{
    public class DeathState : DeathStateBase<AttackerBoyController>
    {
        public DeathState(AttackerBoyController controller) : base(controller) { }

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