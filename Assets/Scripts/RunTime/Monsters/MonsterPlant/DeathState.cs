using UnityEngine;

namespace Game.Monsters.MonsterPlant
{
    public class DeathState : DeathStateBase<MonsterPlantController>
    {
        public DeathState(MonsterPlantController controller) : base(controller) { }

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