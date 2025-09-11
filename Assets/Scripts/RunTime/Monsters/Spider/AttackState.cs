using UnityEngine;

namespace Game.Monsters.Spider
{
    public class AttackState : AttackStateBase<SpiderController>
    {
        public AttackState(SpiderController controller) : base(controller) { }

        public override void OnEnter()
        {
            base.OnEnter();

            //This paremetars are examples,so please change it to your preference!!
            if (attackEndNomTime == 0f) StateFieldSetter.AttackStateFieldSet<SpiderController >(controller, this, clipLength,7,
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