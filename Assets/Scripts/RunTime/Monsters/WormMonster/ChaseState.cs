using UnityEngine;

namespace Game.Monsters.WormMonster
{
    public class ChaseState : ChaseStateBase<WormMonsterController>
    {
        public ChaseState(WormMonsterController controller) : base(controller) { }

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