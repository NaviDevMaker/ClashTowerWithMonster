using Game.Monsters.Slime;
using UnityEngine;

       
namespace Game.Monsters.SlimeKing
{
    public class AttackState : AttackStateBase<SlimeKingController>
    {
        public AttackState(SlimeKingController controller) : base(controller) { }

        public override void OnEnter()
        {
            base.OnEnter();
            if (attackEndNomTime == 0f) StateFieldSetter.AttackStateFieldSet<SlimeKingController>(controller, this, clipLength, 10,
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