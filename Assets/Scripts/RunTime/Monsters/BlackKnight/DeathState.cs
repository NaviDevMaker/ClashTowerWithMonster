using UnityEngine;

namespace Game.Monsters.BlackKnight
{
    public class DeathState : DeathStateBase<BlackKnightController>
    {
        public DeathState(BlackKnightController controller) : base(controller) { }

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