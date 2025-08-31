using UnityEngine;

namespace Game.Monsters.CrabMonster
{
    public class AttackState : AttackStateBase<CrabMonsterController>
    {
        public AttackState(CrabMonsterController controller) : base(controller) { }

        public override void OnEnter()
        {
            base.OnEnter();
            //This paremetars are examples,so please change it to your preference!!
            if (attackEndNomTime == 0f) StateFieldSetter.AttackStateFieldSet<CrabMonsterController >(controller, this, clipLength,13,
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