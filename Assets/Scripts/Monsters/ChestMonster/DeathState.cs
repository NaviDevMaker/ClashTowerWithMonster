using UnityEngine;

       
namespace Game.Monsters.ChestMonster
{
    public class DeathState : DeathStateBase<ChestMonsterContoller>
    {
        public DeathState(ChestMonsterContoller controller) : base(controller) { }

        public override void OnEnter()
        {
            stateAnimSpeed = 0.75f;
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