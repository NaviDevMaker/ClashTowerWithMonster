using UnityEngine;

       
namespace Game.Players.Sword
{
    public class DeathState : DeathStateBase<SwordPlayerController>
    {
        public DeathState(SwordPlayerController controller) : base(controller) { }
        float swordDeathAnimSpeed = 0.5f;
        public override void OnEnter()
        {
            stateAnimSpeed = swordDeathAnimSpeed;
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