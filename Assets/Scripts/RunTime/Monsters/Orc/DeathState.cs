using UnityEngine;

namespace Game.Monsters.Orc
{
    public class DeathState : DeathStateBase<OrcController>
    {
        public DeathState(OrcController controller) : base(controller) { }

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