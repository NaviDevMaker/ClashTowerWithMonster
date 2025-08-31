using UnityEngine;

namespace Game.Monsters.RatAssassin
{
    public class DeathState : DeathStateBase<RatAssassinController>
    {
        public DeathState(RatAssassinController controller) : base(controller) { }

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