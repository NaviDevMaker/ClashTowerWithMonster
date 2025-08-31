using UnityEngine;

namespace Game.Monsters.Skeleton
{
    public class DeathState : DeathStateBase<SkeletonController>
    {
        public DeathState(SkeletonController controller) : base(controller) { }

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