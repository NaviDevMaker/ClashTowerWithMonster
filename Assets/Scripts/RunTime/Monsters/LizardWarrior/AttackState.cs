using UnityEngine;

namespace Game.Monsters.LizardWarrior
{
    public class AttackState : AttackStateBase<LizardWarriorController>
    {
        public AttackState(LizardWarriorController controller) : base(controller) { }

        public override void OnEnter()
        {
            base.OnEnter();

            //This paremetars are examples,so please change it to your preference!!
            if (attackEndNomTime == 0f) StateFieldSetter.AttackStateFieldSet<LizardWarriorController >(controller, this, clipLength,10,
                controller.MonsterStatus.AttackInterval);
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