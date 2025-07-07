using UnityEngine;

namespace Game.Monsters.SpeederBoy
{
    public class AttackState : AttackStateBase<SpeederBoyController>
    {
        public AttackState(SpeederBoyController controller) : base(controller) { }

        public override void OnEnter()
        {
            base.OnEnter();

            //This paremetars are examples,so please change it to your preference!!
            StateFieldSetter.AttackStateFieldSet<SpeederBoyController>(controller, this, clipLength, 8, 1.0f);      
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