using Game.Monsters.Slime;
using UnityEngine;

       
namespace Game.Monsters.ChestMonster
{
    public class AttackState : AttackStateBase<ChestMonsterContoller>
    {
        public AttackState(ChestMonsterContoller controller) : base(controller) { }

        public override void OnEnter()
        {
            base.OnEnter();
            if (attackEndNomTime == 0f) StateFieldSetter.AttackStateFieldSet<ChestMonsterContoller>(controller, this, clipLength, 12, 2.0f);
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