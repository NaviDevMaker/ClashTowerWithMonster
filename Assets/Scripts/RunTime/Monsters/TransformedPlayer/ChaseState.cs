using UnityEngine;

namespace Game.Monsters.TransformedPlayer
{
    public class ChaseState : ChaseStateBase<TransformedPlayer>
    {
        public ChaseState(TransformedPlayer controller) : base(controller) { }

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