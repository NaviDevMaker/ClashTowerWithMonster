using UnityEngine;

namespace Game.Monsters.Salamander
{
    public class DeathState : DeathStateBase<SalamanderController>
    {
        public DeathState(SalamanderController controller) : base(controller) { }

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