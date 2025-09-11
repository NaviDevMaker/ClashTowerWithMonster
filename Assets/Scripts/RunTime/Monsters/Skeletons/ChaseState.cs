using UnityEngine;

namespace Game.Monsters.Skeleton
{
    public class ChaseState : ChaseStateBase<SkeletonController>
    {
        public ChaseState(SkeletonController controller) : base(controller) { }

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