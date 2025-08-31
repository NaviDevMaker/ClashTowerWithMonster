using UnityEngine;

namespace Game.Monsters.NagaWizard
{
    public class DeathState : DeathStateBase<NagaWizardController>
    {
        public DeathState(NagaWizardController controller) : base(controller) { }

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