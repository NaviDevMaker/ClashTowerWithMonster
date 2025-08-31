using UnityEngine;

namespace Game.Monsters.Spider
{
    public class ChaseState : ChaseStateBase<SpiderController>
    {
        public ChaseState(SpiderController controller) : base(controller) { }

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