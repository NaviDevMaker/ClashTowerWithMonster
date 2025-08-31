using UnityEngine;

namespace Game.Monsters.BishopKnight
{
    public class DeathState : DeathStateBase<BishopKnightController>
    {
        public DeathState(BishopKnightController controller) : base(controller) { }

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