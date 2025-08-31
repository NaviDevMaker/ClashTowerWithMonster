using UnityEngine;

namespace Game.Monsters.WormMonster
{
    public class DeathState : DeathStateBase<WormMonsterController>
    {
        public DeathState(WormMonsterController controller) : base(controller) { }

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