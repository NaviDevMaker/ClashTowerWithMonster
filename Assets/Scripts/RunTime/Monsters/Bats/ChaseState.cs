using UnityEngine;

namespace Game.Monsters.Bat
{
    public class ChaseState : ChaseStateBase<BatController>
    {
        public ChaseState(BatController controller) : base(controller) { }

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