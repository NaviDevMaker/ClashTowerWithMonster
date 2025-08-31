using UnityEngine;

namespace Game.Monsters.Fishman
{
    public class ChaseState : ChaseStateBase<FishmanController>
    {
        public ChaseState(FishmanController controller) : base(controller) { }

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