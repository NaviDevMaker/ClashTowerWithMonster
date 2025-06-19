using UnityEngine;

namespace Game.Monsters.CactusMonster
{
    public class DeathState : DeathStateBase<CactusMonsterController>
    {
        public DeathState(CactusMonsterController controller) : base(controller) { }

        public override void OnEnter()
        {
            stateAnimSpeed = 1.0f;
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