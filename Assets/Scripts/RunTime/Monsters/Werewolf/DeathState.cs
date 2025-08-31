using UnityEngine;

namespace Game.Monsters.Werewolf
{
    public class DeathState : DeathStateBase<WerewolfController>
    {
        public DeathState(WerewolfController controller) : base(controller) { }

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