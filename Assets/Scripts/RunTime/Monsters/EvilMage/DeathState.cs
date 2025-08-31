using UnityEngine;

namespace Game.Monsters.EvilMage
{
    public class DeathState : DeathStateBase<EvilMageController>
    {
        public DeathState(EvilMageController controller) : base(controller) { }

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