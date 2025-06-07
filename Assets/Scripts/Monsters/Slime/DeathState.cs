using UnityEngine;

namespace Game.Monsters.Slime
{
    public class DeathState : DeathStateBase<SlimeController>
    {
        public DeathState(SlimeController slime) : base(slime) { }

        public override void OnEnter()
        {
            stateAnimSpeed = 1.0f;
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

