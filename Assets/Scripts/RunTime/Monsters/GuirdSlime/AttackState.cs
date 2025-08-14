using Game.Monsters.Slime;
using UnityEngine;

namespace Game.Monsters.GuirdSlime
{
    public class AttackState : AttackStateBase<GuirdSlimeController>
    {
        public AttackState(GuirdSlimeController slime) : base(slime) { }

        public override void OnEnter()
        {
            base.OnEnter();

            if (attackEndNomTime == 0f) StateFieldSetter.AttackStateFieldSet<GuirdSlimeController>(controller, this, clipLength, 12,
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

