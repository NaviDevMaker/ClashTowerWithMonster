using UnityEngine;

namespace Game.Monsters.Golem
{
    public class DeathState : DeathStateBase<GolemController>
    {
        public DeathState(GolemController controller) : base(controller) { }

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