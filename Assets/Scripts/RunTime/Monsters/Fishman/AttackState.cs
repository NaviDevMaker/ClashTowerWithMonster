using UnityEngine;

namespace Game.Monsters.Fishman
{
    public class AttackState : AttackStateBase<FishmanController>
    {
        public AttackState(FishmanController controller) : base(controller) { }

        public override void OnEnter()
        {
            base.OnEnter();

            //This paremetars are examples,so please change it to your preference!!
            if (attackEndNomTime == 0f) StateFieldSetter.AttackStateFieldSet<FishmanController >(controller, this, clipLength,21,
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