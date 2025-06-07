using Game.Monsters.Slime;
using UnityEngine;

       
namespace Game.Monsters.BigEyeMonster
{
    public class AttackState : AttackStateBase<BigEyeBallMonsterController>
    {
        public AttackState(BigEyeBallMonsterController controller) : base(controller) { }

        public override void OnEnter()
        {
            base.OnEnter();
            if (attackEndNomTime == 0f) StateFieldSetter.AttackStateFieldSet<BigEyeBallMonsterController>(controller, this, clipLength, 15, 0.5f);
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