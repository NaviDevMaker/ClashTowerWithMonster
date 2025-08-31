using UnityEngine;

namespace Game.Monsters.Specter
{
    public class ChaseState : ChaseStateBase<SpecterController>
    {
        public ChaseState(SpecterController controller) : base(controller) { }

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