using UnityEngine;

namespace Game.Monsters.SpeederBoy
{
    public class DeathState : DeathStateBase<SpeederBoyController>
    {
        public DeathState(SpeederBoyController controller) : base(controller) { }

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