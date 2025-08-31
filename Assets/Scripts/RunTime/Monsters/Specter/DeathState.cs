using UnityEngine;

namespace Game.Monsters.Specter
{
    public class DeathState : DeathStateBase<SpecterController>
    {
        public DeathState(SpecterController controller) : base(controller) { }

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