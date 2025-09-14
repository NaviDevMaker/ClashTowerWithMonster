using UnityEngine;

namespace Game.Monsters.TransformedPlayer
{
    public class DeathState : DeathStateBase<TransformedPlayer>
    {
        public DeathState(TransformedPlayer controller) : base(controller) { }

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