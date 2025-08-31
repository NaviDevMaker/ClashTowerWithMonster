using UnityEngine;

namespace Game.Monsters.StingRay
{
    public class DeathState : DeathStateBase<StingRayController>
    {
        public DeathState(StingRayController controller) : base(controller) { }

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