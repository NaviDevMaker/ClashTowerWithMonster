using UnityEngine;

namespace Game.Monsters.DemonKing
{
    public class DeathState : DeathStateBase<DemonKingController>
    {
        public DeathState(DemonKingController controller) : base(controller) { }

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