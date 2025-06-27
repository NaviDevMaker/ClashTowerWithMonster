using UnityEngine;

namespace Game.Monsters.EvilMushroom
{
    public class ChaseState : ChaseStateBase<EvilMushroomController>
    {
        public ChaseState(EvilMushroomController controller) : base(controller) { }

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