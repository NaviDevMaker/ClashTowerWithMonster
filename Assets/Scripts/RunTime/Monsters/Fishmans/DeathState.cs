using UnityEngine;

namespace Game.Monsters.Fishman
{
    public class DeathState : DeathStateBase<FishmanController>
    {
        public DeathState(FishmanController controller) : base(controller) { }

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