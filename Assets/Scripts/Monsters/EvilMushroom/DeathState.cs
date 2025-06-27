using UnityEngine;

namespace Game.Monsters.EvilMushroom
{
    public class DeathState : DeathStateBase<EvilMushroomController>
    {
        public DeathState(EvilMushroomController controller) : base(controller) { }

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