using UnityEngine;

       
namespace Game.Monsters.BigEyeMonster
{
    public class DeathState : DeathStateBase<BigEyeBallMonsterController>
    {
        public DeathState(BigEyeBallMonsterController controller) : base(controller) { }

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