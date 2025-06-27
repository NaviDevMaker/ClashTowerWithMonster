using UnityEngine;

       
namespace Game.Monsters.BigEyeMonster
{
    public class ChaseState : ChaseStateBase<BigEyeBallMonsterController>
    {
        public ChaseState(BigEyeBallMonsterController controller) : base(controller) { }

        public override void OnEnter()
        {
            if (flyingOffsetY == 0) flyingOffsetY = controller.FlyingMonsterStatus.FlyingOffsetY;
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