using UnityEngine;

       
namespace Game.Players.Sword
{
    public class CheckEnemyState : CheckEnemyStateBase<SwordPlayerController>
    {
        public CheckEnemyState(SwordPlayerController controller) : base(controller) { }

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