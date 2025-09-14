using UnityEngine;

namespace Game.Monsters.TransformedPlayer
{
    public class AttackState : AttackStateBase<TransformedPlayer>
    {
        public AttackState(TransformedPlayer controller) : base(controller) { }

        public override void OnEnter()
        {
            base.OnEnter();

            //This paremetars are examples,so please change it to your preference!!
            if (attackEndNomTime == 0f) StateFieldSetter.AttackStateFieldSet<TransformedPlayer >(controller, this, clipLength,7,
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