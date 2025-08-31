using UnityEngine;

namespace Game.Monsters.Golem
{
    public class ChaseState : ChaseStateBase<GolemController>
    {
        public ChaseState(GolemController controller) : base(controller) { }

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