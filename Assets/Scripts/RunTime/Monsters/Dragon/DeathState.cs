using UnityEngine;

namespace Game.Monsters.Dragon
{
    public class DeathState : DeathStateBase<DragonController>
    {
        public DeathState(DragonController controller) : base(controller) { }

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