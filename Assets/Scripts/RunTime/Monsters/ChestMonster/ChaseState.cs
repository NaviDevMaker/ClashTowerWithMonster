using UnityEngine;

       
namespace Game.Monsters.ChestMonster
{
    public class ChaseState : ChaseStateBase<ChestMonsterContoller>
    {
        public ChaseState(ChestMonsterContoller controller) : base(controller) { }

        public override void OnEnter()
        {
            base.OnEnter();
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