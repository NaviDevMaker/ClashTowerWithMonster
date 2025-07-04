using UnityEngine;

namespace Game.Monsters.Slime
{
    public class AttackState : AttackStateBase<SlimeController>
    {
        public AttackState(SlimeController slime) : base(slime) { }

        public override void OnEnter()
        {
            base.OnEnter();

            if(attackEndNomTime == 0f) StateFieldSetter.AttackStateFieldSet<SlimeController>(controller, this, clipLength, 10, 1.0f);      
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

