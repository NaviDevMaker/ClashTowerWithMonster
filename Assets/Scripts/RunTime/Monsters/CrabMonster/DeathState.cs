using UnityEngine;

namespace Game.Monsters.CrabMonster
{
    public class DeathState : DeathStateBase<CrabMonsterController>
    {
        public DeathState(CrabMonsterController controller) : base(controller) { }

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