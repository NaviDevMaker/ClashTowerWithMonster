using UnityEngine;

namespace Game.Monsters.Cyclops
{
    public class DeathState : DeathStateBase<CyclopsController>
    {
        public DeathState(CyclopsController controller) : base(controller) { }

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