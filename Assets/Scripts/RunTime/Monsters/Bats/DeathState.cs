using UnityEngine;

namespace Game.Monsters.Bat
{
    public class DeathState : DeathStateBase<BatController>
    {
        public DeathState(BatController controller) : base(controller) { }

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