using UnityEngine;

namespace Game.Monsters.FylingDemon
{
    public class ChaseState : ChaseStateBase<FylingDemonController>
    {
        public ChaseState(FylingDemonController controller) : base(controller) { }

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