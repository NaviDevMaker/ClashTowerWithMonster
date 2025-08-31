using UnityEngine;

namespace Game.Monsters.FylingDemon
{
    public class AttackState : AttackStateBase<FylingDemonController>
    {
        public AttackState(FylingDemonController controller) : base(controller) { }

        public override void OnEnter()
        {
            base.OnEnter();

            //This paremetars are examples,so please change it to your preference!!
            if (attackEndNomTime == 0f) StateFieldSetter.AttackStateFieldSet<FylingDemonController >(controller, this, clipLength,10,
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