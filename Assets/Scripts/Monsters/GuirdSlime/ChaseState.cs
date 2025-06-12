using UnityEngine;

namespace Game.Monsters.GuirdSlime
{
    public class ChaseState : ChaseStateBase<GuirdSlimeController>
    {
        public ChaseState(GuirdSlimeController slime) : base(slime) { }

        public override void OnEnter()
        {
            base.OnEnter();
        }
        public override void OnUpdate()
        {
            base .OnUpdate(); 
        }
        public override void OnExit()
        {
            base.OnExit();
        }
    }

}

