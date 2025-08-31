using UnityEngine;

namespace Game.Monsters.FylingDemon
{
    public class DeathState : DeathStateBase<FylingDemonController>
    {
        public DeathState(FylingDemonController controller) : base(controller) { }

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