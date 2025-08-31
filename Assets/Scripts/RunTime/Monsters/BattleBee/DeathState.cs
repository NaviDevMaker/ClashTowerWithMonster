using UnityEngine;

namespace Game.Monsters.BattleBee
{
    public class DeathState : DeathStateBase<BattleBeeController>
    {
        public DeathState(BattleBeeController controller) : base(controller) { }

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