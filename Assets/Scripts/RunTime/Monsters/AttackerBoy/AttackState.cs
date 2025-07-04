using UnityEngine;

namespace Game.Monsters.AttackerBoy
{
    public class AttackState : AttackStateBase<AttackerBoyController>
    {
        public AttackState(AttackerBoyController controller) : base(controller) { }

        public override void OnEnter()
        {
            base.OnEnter();
            //This paremetars are examples,so please change it to your preference!!
            if (attackEndNomTime == 0f) StateFieldSetter.AttackStateFieldSet<AttackerBoyController>(controller, this, clipLength, 8, 1.0f);
            Debug.Log(interval);
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