using UnityEngine;

namespace Game.Monsters.EvilMage
{
    public class AttackState : AttackStateBase<EvilMageController>
    {
        public AttackState(EvilMageController controller) : base(controller) { }

        public override void OnEnter()
        {
            base.OnEnter();

            //This paremetars are examples,so please change it to your preference!!
            if (attackEndNomTime == 0f) StateFieldSetter.AttackStateFieldSet<EvilMageController >(controller, this, clipLength,10,
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