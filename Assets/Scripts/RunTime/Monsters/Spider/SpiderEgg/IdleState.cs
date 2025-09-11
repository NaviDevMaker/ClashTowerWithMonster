using UnityEngine;
using Cysharp.Threading.Tasks;
namespace Game.Monsters.SpiderEgg
{
    public class IdleState : IdleStateBase<SpiderEggController>
    {
        public IdleState(SpiderEggController controller) : base(controller) { }
        public override void OnEnter()
        {
            OnEnterProcess().Forget();
        }
        public override void OnUpdate() 
        { 
            if(isEndSummon)
            {
                nextState = controller.SwayState;
                controller.ChangeState(nextState);
            }
        }
        public override void OnExit() { }
        protected override void AllResetBoolProparty() { }
    }

}