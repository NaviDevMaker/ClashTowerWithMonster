using UnityEngine;

namespace Game.Monsters.GuirdSlime
{
    public class DeathState : DeathStateBase<GuirdSlimeController>
    {
        public DeathState(GuirdSlimeController slime) : base(slime) { }

        public override void OnEnter()
        {
            stateAnimSpeed = 1f;
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

