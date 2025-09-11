using UnityEngine;

namespace Game.Monsters.SpiderEgg
{
    public class DeathState : DeathStateBase<SpiderEggController>
    {
        public DeathState(SpiderEggController controller) : base(controller) { }

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