using UnityEngine;

namespace Game.Monsters.NagaWizard
{
    public class AttackState : AttackStateBase<NagaWizardController>
    {
        public AttackState(NagaWizardController controller) : base(controller) { }

        public override void OnEnter()
        {
            base.OnEnter();

            //This paremetars are examples,so please change it to your preference!!
            if (attackEndNomTime == 0f) StateFieldSetter.AttackStateFieldSet<NagaWizardController >(controller, this, clipLength,10,
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