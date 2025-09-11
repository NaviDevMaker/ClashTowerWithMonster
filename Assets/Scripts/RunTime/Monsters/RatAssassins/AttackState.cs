using UnityEngine;

namespace Game.Monsters.RatAssassin
{
    public class AttackState : AttackStateBase<RatAssassinController>
    {
        public AttackState(RatAssassinController controller) : base(controller) { }

        public override void OnEnter()
        {
            base.OnEnter();

            //This paremetars are examples,so please change it to your preference!!
            if (attackEndNomTime == 0f) StateFieldSetter.AttackStateFieldSet<RatAssassinController >(controller, this, clipLength,10,
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