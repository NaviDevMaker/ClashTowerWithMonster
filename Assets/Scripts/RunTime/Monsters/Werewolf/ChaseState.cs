using UnityEngine;

namespace Game.Monsters.Werewolf
{
    public class ChaseState : ChaseStateBase<WerewolfController>
    {
        public ChaseState(WerewolfController controller) : base(controller) { }

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