using UnityEngine;

namespace Game.Monsters.CactusMonster
{
    public class AttackState : AttackStateBase<CactusMonsterController>
    {
        public AttackState(CactusMonsterController controller) : base(controller) { }

        public override void OnEnter()
        {
            base.OnEnter();

            //This paremetars are examples,so please change it to your preference!!
            if(attackEndNomTime == 0f) StateFieldSetter.AttackStateFieldSet<CactusMonsterController>(controller, this, clipLength, 10,
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