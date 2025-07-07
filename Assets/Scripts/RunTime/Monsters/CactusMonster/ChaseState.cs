using UnityEngine;

namespace Game.Monsters.CactusMonster
{
    public class ChaseState : ChaseStateBase<CactusMonsterController>
    {
        public ChaseState(CactusMonsterController controller) : base(controller) { }

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