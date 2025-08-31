using UnityEngine;

namespace Game.Monsters.LizardWarrior
{
    public class DeathState : DeathStateBase<LizardWarriorController>
    {
        public DeathState(LizardWarriorController controller) : base(controller) { }

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